//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TerminalGuiDesigner.UI.Windows
{
    using Terminal.Gui;
    using System.Linq;
    using TerminalGuiDesigner;
    using TerminalGuiDesigner.ToCode;
    using TerminalGuiDesigner.UI.Windows;

    public partial class PosEditor : Dialog {

        public Pos Result { get; private set; }
        public bool Cancelled { get; private set; }

        public Design Design { get; }
        public Property Property { get; }

        public PosEditor(Design design, Property property) {
            InitializeComponent();
            
            Design = design;
            Property = property;


            Title = "Pos Designer";
            Border.BorderStyle = BorderStyle.Double;

            rgPosType.KeyPress += RgPosType_KeyPress;

            btnOk.Clicked += BtnOk_Clicked;
            btnCancel.Clicked += BtnCancel_Clicked;
            Cancelled = true;
            Modal = true;

            var siblings = Design.GetSiblings().ToList();

            ddRelativeTo.SetSource(siblings);

            var val = (Pos)property.GetValue();
            if(val.GetPosType(siblings,out var type,out var value,out var relativeTo,out var side, out var offset))
            {
                switch(type)
                {
                    case PosType.Absolute:
                        rgPosType.SelectedItem = 0;
                        break;
                    case PosType.Percent:
                        rgPosType.SelectedItem = 1;
                        break;
                    case PosType.Relative:
                        rgPosType.SelectedItem = 2;
                        break;
                    case PosType.Center:
                        rgPosType.SelectedItem = 3;
                        
                        // TODO: once the current 'main' branch is published to nuget package we can do this
                        //ddRelativeTo.SelectedItem = siblings.IndexOf(relativeTo);
                        break;
                }

                tbValue.Text = value.ToString();
                tbOffset.Text = offset.ToString();
            }

            SetupForCurrentPosType();

            rgPosType.SelectedItemChanged += DdType_SelectedItemChanged;

            ddSide.SetSource(Enum.GetValues(typeof(Side)).Cast<Enum>().ToList());
        }

        private void RgPosType_KeyPress(KeyEventEventArgs obj)
        {
            var c = (char)obj.KeyEvent.KeyValue;
            
            // if user types in some text change the focus to the text box to enable entering digits
            if ((obj.KeyEvent.Key == Key.Backspace || char.IsDigit(c)) && tbValue.Visible)
            {
                tbValue?.FocusFirst();
            }            
        }

        private void DdType_SelectedItemChanged(RadioGroup.SelectedItemChangedArgs obj)
        {
            SetupForCurrentPosType();            
        }

        private void SetupForCurrentPosType()
        {
            
            switch(GetPosType())
            {
                case PosType.Anchor:
                case PosType.Percent:
                    lblRelativeTo.Visible = false;
                    ddRelativeTo.Visible = false;
                    lblSide.Visible = false;
                    ddSide.Visible = false;
                    
                    lblValue.Y = 1;
                    lblValue.Visible = true;
                    tbValue.Visible = true;
                    
                    lblOffset.Y = 3;
                    lblOffset.Visible = true;
                    
                    // TODO: We really shouldn't have to do this try removing it but it causes
                    // Exception with current Terminal.Gui nuget package (2022-04-12).  Later down
                    // the line try removing and hammering the radio buttons to see if the problem
                    // has disapeared.  It breaks the tab order for one :(
                    Remove(tbOffset);
                    tbOffset.Y = 3;
                    tbOffset.Visible = true;
                    Add(tbOffset);

                    SetNeedsDisplay();
                    break;
                case PosType.Center:
                    lblRelativeTo.Visible = false;
                    ddRelativeTo.Visible = false;
                    lblSide.Visible = false;
                    ddSide.Visible = false;
                    
                    lblValue.Visible = false;
                    tbValue.Visible = false;
                    
                    lblOffset.Y = 1;
                    lblOffset.Visible = true;
                    Remove(tbOffset);
                    tbOffset.Y = 1;
                    tbOffset.Visible = true;
                    Add(tbOffset);

                    SetNeedsDisplay();
                    break;
                case PosType.Absolute:
                    ddRelativeTo.Visible = false;
                    lblRelativeTo.Visible = false;
                    lblSide.Visible = false;
                    ddSide.Visible = false;

                    lblValue.Y = 1;
                    lblValue.Visible = true;
                    tbValue.Visible = true;

                    lblOffset.Visible = false;
                    tbOffset.Visible = false;
                    SetNeedsDisplay();
                    break;
                case PosType.Relative:
                    lblRelativeTo.Y = 1;
                    lblRelativeTo.Visible = true;
                    ddRelativeTo.Y = 1;
                    ddRelativeTo.Visible = true;

                    lblSide.Y = 3;
                    lblSide.Visible = true;

                    this.Remove(ddSide);
                    ddSide.IsInitialized = false;
                    ddSide.Y = 3;
                    ddSide.Visible = true;
                    ddSide.IsInitialized = true;
                    this.Add(ddSide);

                    lblValue.Visible = false;
                    tbValue.Visible = false;

                    lblOffset.Y = 5;
                    lblOffset.Visible = true;
                    tbOffset.Y = 5;
                    tbOffset.Visible = true;
                    SetNeedsDisplay();
                    break;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void BtnCancel_Clicked()
        {
            Cancelled = true;
            Application.RequestStop();
        }

        private void BtnOk_Clicked()
        {
            Cancelled = !BuildPos(out var result);
            Result = result;
            Application.RequestStop();
        }

        public bool BuildPos(out Pos result)
        {
            // pick what type of Pos they want
            var type = GetPosType();

            switch (type)
            {
                case PosType.Absolute:
                    return BuildPosAbsolute(out result);
                case PosType.Relative:
                    return BuildPosRelative(out result);
                case PosType.Percent:
                    return BuildPosPercent(out result);
                case PosType.Center:
                    return BuildPosCenter(out result);
                case PosType.Anchor: throw new NotImplementedException();

                default: throw new ArgumentOutOfRangeException();

            }
        }

        private PosType GetPosType()
        {
            return Enum.Parse<PosType>(rgPosType.RadioLabels[rgPosType.SelectedItem].ToString());
        }


        private Side? GetSide()
        {
            return ddSide.SelectedItem == -1 ? null : (Side)ddSide.Source.ToList()[ddSide.SelectedItem];
        }

        private bool GetOffset(out int offset)
        {
            return int.TryParse(tbOffset.Text.ToString(),out offset);
        }

        private bool BuildPosRelative(out Pos result)
        {
            var relativeTo = ddRelativeTo.SelectedItem == -1 ? null : ddRelativeTo.Source.ToList()[ddRelativeTo.SelectedItem] as Design;

            if (relativeTo != null)
            {
                var side = GetSide();

                if (side != null)
                {
                    Pos pos;
                    switch (side)
                    {
                        case Side.Top:
                            pos = Pos.Top(relativeTo.View);
                            break;
                        case Side.Bottom:
                            pos = Pos.Bottom(relativeTo.View);
                            break;
                        case Side.Left:
                            pos = Pos.Left(relativeTo.View);
                            break;
                        case Side.Right:
                            pos = Pos.Right(relativeTo.View);
                            break;
                        default: throw new ArgumentOutOfRangeException(nameof(side));
                    }

                    if (GetOffset(out int offset))
                    {
                        if(offset != 0)
                        {
                            result = pos + offset;   
                            return true;
                        }
                    }

                    result = pos;
                    return true;
                }
            }

            // Its got no side or no relative to control
            result = null;
            return false;
        }

        private bool BuildPosAbsolute(out Pos result)
        {
            if (GetValue(out int newPos))
            {
                result = Pos.At(newPos);
                return true;
            }

            result = null;
            return false;
        }

        private bool GetValue(out int newPos)
        {
            return int.TryParse(tbValue.Text.ToString(),out newPos);
        }

        private bool BuildPosPercent(out Pos result)
        {
            if (GetValue(out int newPercent))
            {
                result = Pos.Percent(newPercent);

                if (GetOffset(out int offset) && offset != 0)
                {
                    result = result + offset;
                    return true;
                }

                return true;
            }

            result = null;
            return false;
        }
        private bool BuildPosCenter(out Pos result)
        {
            result = Pos.Center();

            if (GetOffset(out int offset) && offset != 0)
            {
                result = result + offset;
                return true;
            }

            return true;
        }
    }
}
