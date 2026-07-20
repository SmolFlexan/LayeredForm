# LayeredForm

Transparent windows in C#. You can also click through transparent regions.

## Usage

```csharp
using SmolFlexan;

public partial class Form1 : LayeredForm {
    public Window()
    {
        InitializeComponent();

        FormBorderStyle = FormBorderStyle.None;
        DoubleBuffered = true;
    }

    // you can also use the `Load` event listener
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e); // if overriding `OnLoad`, it's important you call base method

        InitLayer(Width, Height);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);

        // this can be in your drawing loop (`OnPaint` doesn't work)
        using (Graphics g = GetGraphics())
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);

            g.DrawLine(Pens.AliceBlue, 0, 0, Width, Height);
        }

        UpdateLayer(); // instructs to update the transparent layer
    }
}
```
