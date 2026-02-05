// See https://aka.ms/new-console-template for more information

using System.Numerics;
using GpuSpecializationCapstone;
using GpuSpecializationCapstone.Texture;
using ImGuiNET;
using NativeFileDialogSharp;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;
using Color = System.Drawing.Color;
using Window = Silk.NET.Windowing.Window;
using static GpuSpecializationCapstone.GUI.ImGuiControls;

IReadOnlyList<TextureMagFilter> magFilterOpenGl = [ TextureMagFilter.Linear, TextureMagFilter.Nearest ];
int selectedOpenGlMagFilter = 0;

IReadOnlyList<TextureMinFilter> minFilterOpenGl = [TextureMinFilter.Linear, TextureMinFilter.Nearest];
int selectedOpenGlMinFilter = 0;

IReadOnlyList<DownsizerType> downsizerTypes = [DownsizerType.Linear, DownsizerType.Nearest];
int selectedDownsizerType = 0;

float textureScaleFactor = 1;

int selectedLevel = 0;
Dictionary<int, OpenGlTexture> mipLevels = new();

ImGuiController? controller = null;
IInputContext? inputContext = null;

Downsampler generator = new();
GL gl = null!;
using var window = Window.Create(WindowOptions.Default);

generator.OnRunFinished += () =>
{
    // Read levels
    for (int i = 0; i < generator.Levels; i++)
    {
        // If it already exists we need to dispose of texture.
        if (mipLevels.TryGetValue(i, out OpenGlTexture? texture))
        {
            texture.Dispose();
        }

        // Read image pixels and create new texture.
        byte[] pixels = generator.ReadImage(i, out uint width, out uint height);
        // Note: Texture cannot be initialized here. It must be initialized on main thread.
        mipLevels[i] = new OpenGlTexture(gl, pixels, width, height);
    }
};

// Our loading function
window.Load += () =>
{
    controller = new ImGuiController(
        gl = window.CreateOpenGL(), // load OpenGL
        window, // pass in our window
        inputContext = window.CreateInput() // create an input context
    );
};

// Handle resizes
window.FramebufferResize += s =>
{
    // Adjust the viewport to the new window size
    gl.Viewport(s);
};

Task OpenFileDialogAsync()
{
    return Task.Run(() =>
    {
        DialogResult result = Dialog.FileOpen("png");
        string path = result.Path;
        if (result.IsOk)
        {
            if (path.EndsWith(".png"))
            {
                generator.SetSource(path);
                generator.Run();
            }
        }
    });
}


void MipLevelsSelectDropdown()
{
    if (mipLevels.Count == 0)
    {
        return;
    }

    ImGui.PushItemWidth(200);
    // Preview dropdown.
    if (ImGui.BeginCombo("Mip Level Preview", "Level " + selectedLevel))
    {
        for (int level = 0; level < mipLevels.Count; level++)
        {
            bool selected = (selectedLevel == level);
            if (ImGui.Selectable("Level " + level, selected))
            {
                selectedLevel = level;
            }

            if (selected)
            {
                ImGui.SetItemDefaultFocus();
            }
        }

        ImGui.EndCombo();
    }

    ImGui.PopItemWidth();

    OpenGlTexture texture = mipLevels[selectedLevel];
    texture.Initialize();
    texture.ChangeMagFilter(magFilterOpenGl[selectedOpenGlMagFilter]);
    texture.ChangeMinFilter(minFilterOpenGl[selectedOpenGlMinFilter]);
    DrawClampedTexture(texture, 200, 200);
}


void ChangeTexturesMagFilter(int selectedIndex)
{
    TextureMagFilter magFilter = magFilterOpenGl[selectedIndex];
    foreach (OpenGlTexture texture in mipLevels.Values)
    {
        texture.ChangeMagFilter(magFilter);
    }
}

void ChangeTexturesMinFilter(int selectedIndex)
{
    TextureMinFilter minFilter = minFilterOpenGl[selectedIndex];
    foreach (OpenGlTexture texture in mipLevels.Values)
    {
        texture.ChangeMinFilter(minFilter);
    }
}

void ChangeDownsizerType(int selectedIndex)
{
    DownsizerType type = downsizerTypes[selectedIndex];
    generator.DownsizerType = type;
    generator.Run();
}

// The render function
window.Render += delta =>
{
    // Make sure ImGui is up-to-date
    controller.Update((float)delta);

    bool zoomModifier = ImGui.IsKeyDown(ImGuiKey.LeftCtrl);

    if (zoomModifier)
    {
        ImGuiIOPtr io = ImGui.GetIO();
        float wheel = io.MouseWheel;
        if (wheel != 0.0f)
        {
            textureScaleFactor += 0.05f * wheel;
            if (textureScaleFactor < 0)
            {
                textureScaleFactor = 0;
            }
        }
    }
    
    gl.ClearColor(Color.FromArgb(255, (int)(.45f * 255), (int)(.55f * 255), (int)(.60f * 255)));
    gl.Clear((uint)ClearBufferMask.ColorBufferBit);

    int menuWidth = 350;
    
    ImGui.Begin("Texture",
        zoomModifier
            ? ImGuiWindowFlags.NoMouseInputs
            : ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.AlwaysHorizontalScrollbar);

    if (mipLevels.Count > 0)
    {
        OpenGlTexture texture = mipLevels[selectedLevel];
        DrawTexture(texture, textureScaleFactor);
    }

    ImGui.End();
    
    
    ImGui.Begin("Options");

    // Draw Image over entire ui
    if (mipLevels.Count > 0)
    {
        OpenGlTexture texture = mipLevels[selectedLevel];
    }

    if (ImGui.Button("Add Image"))
    {
        _ = OpenFileDialogAsync();
    }

    MipLevelsSelectDropdown();

    ImGui.InputFloat("Texture Scale", ref textureScaleFactor);

    ComboBox("OpenCL Downsize Algorithm",
        downsizerTypes,
        downsizerTypes[selectedDownsizerType].ToString(),
        ref selectedDownsizerType,
        ChangeDownsizerType
    );
    ComboBox("OpenGL Mag Filter", magFilterOpenGl,
        magFilterOpenGl[selectedOpenGlMagFilter].ToString(),
        ref selectedOpenGlMagFilter,
        ChangeTexturesMagFilter);
    ComboBox("OpenGL Min Filter", minFilterOpenGl,
        minFilterOpenGl[selectedOpenGlMinFilter].ToString(),
        ref selectedOpenGlMinFilter,
        ChangeTexturesMinFilter
    );
    ImGui.End();
    
    // Make sure ImGui renders too!
    controller.Render();
};

// The closing function
window.Closing += () =>
{
    // Dispose our controller first
    controller?.Dispose();

    // Dispose the input context
    inputContext?.Dispose();

    // Unload OpenGL
    gl?.Dispose();
};

// Now that everything's defined, let's run this bad boy!
window.Run();