//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YourNamespace {
    using System;
    using Terminal.Gui;
    
    
    public partial class MyWindow : Terminal.Gui.Window {
        
        private Terminal.Gui.GraphView graphview1;
        
        private void InitializeComponent() {
            this.Text = "";
            this.Width = Dim.Fill(0);
            this.Height = Dim.Fill(0);
            this.X = 0;
            this.Y = 0;
            this.TextAlignment = TextAlignment.Centered;
            this.Title = "Welcome to Demo";
            this.graphview1 = new Terminal.Gui.GraphView();
            this.graphview1.Data = "graphview1";
            this.graphview1.Text = "";
            this.graphview1.Width = 20;
            this.graphview1.Height = 5;
            this.graphview1.X = 4;
            this.graphview1.Y = 1;
            this.graphview1.TextAlignment = TextAlignment.Left;
            this.graphview1.GraphColor = Terminal.Gui.Attribute.Make(Color.Magenta,Color.Cyan);
            this.Add(this.graphview1);
        }
    }
}
