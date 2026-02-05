using System.Numerics;
using GpuSpecializationCapstone.Texture;
using ImGuiNET;

namespace GpuSpecializationCapstone.GUI;

/// <summary>
/// The helper controls.
/// </summary>
public static class ImGuiControls
{
    /// <summary>
    /// Creates the imgui combo box.
    /// </summary>
    /// <param name="label">The label to use.</param>
    /// <param name="items">The combobox items.</param>
    /// <param name="preview">The preview item.</param>
    /// <param name="selectedIndex">Selected item index.</param>
    /// <param name="onSelected">Callback for when item is selected.</param>
    internal static void ComboBox<T>(string label, IReadOnlyList<T> items, string preview, ref int selectedIndex, Action<int> onSelected)
    {
        // Preview dropdown.
        ImGui.PushItemWidth(200);
        if (ImGui.BeginCombo(label, preview))
        {
            for (int i = 0; i < items.Count; i++)
            {
                bool selected = (selectedIndex == i);
                if (ImGui.Selectable(items[i]?.ToString(), selected))
                {
                    selectedIndex = i;
                    onSelected(selectedIndex);
                }

                if (selected)
                {
                    ImGui.SetItemDefaultFocus();
                }
            }

            ImGui.EndCombo();
        }

        ImGui.PopItemWidth();
    }
    
    /// <summary>
    /// Draws the OpenGL texture.
    /// </summary>
    /// <param name="texture">The <see cref="OpenGlTexture"/>.</param>
    /// <param name="scale">The texture scale.</param>
    internal static void DrawTexture(OpenGlTexture texture, float scale)
    {
        texture.Initialize();
        IntPtr ptr = new IntPtr(texture.Handle);

        float width = texture.Width;
        float height = texture.Height;

        ImGui.Image(ptr, new Vector2(width * scale, height * scale));
    }
    
    /// <summary>
    /// Draws the OpenGL texture clamped to clamp values.
    /// </summary>
    /// <param name="texture">The <see cref="OpenGlTexture"/>.</param>
    /// <param name="maxWidth">The max width.</param>
    /// <param name="maxHeight">The max height.</param>
    internal static void DrawClampedTexture(OpenGlTexture texture, int maxWidth, int maxHeight)
    {
        IntPtr ptr = new IntPtr(texture.Handle);

        float aspectRatio = (float)texture.Width / texture.Height;
        float width = texture.Width;
        float height = texture.Height;

        if (width > maxWidth && aspectRatio >= 1.0f)
        {
            height *= (maxWidth / width);
            width = maxWidth;
        }
        else if (height > maxHeight && aspectRatio <= 1.0f)
        {
            width *= (maxHeight / height);
            height = maxHeight;
        }

        ImGui.Image(ptr, new Vector2(width, height));
    }

}