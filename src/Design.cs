
using System.CodeDom;
using System.Reflection;
using NLog;
using NStack;
using Terminal.Gui;

namespace TerminalGuiDesigner;
    
public class Design
{
    public string FieldName { get; }

    /// <summary>
    /// The view being designed.  Do not use <see cref="View.Add(Terminal.Gui.View)"/> on
    /// this instance.  Instead use <see cref="AddDesign(string, Terminal.Gui.View)"/> so that
    /// new child controls are preserved for design time changes
    /// </summary>
    public View View {get;}

    /// <summary>
    /// All immediate children of <see cref="View"/> as design time objects.
    /// Use <see cref="AddDesign(string, Terminal.Gui.View)"/> to add new objects
    /// to the <see cref="View"/>
    /// </summary>
    public IReadOnlyCollection<Design> SubControlDesigns => subControlDesigns.AsReadOnly();

    private List<Design> subControlDesigns = new List<Design>();

    private Logger logger = LogManager.GetCurrentClassLogger();

    public Design(string fieldName, View view)
    {
        FieldName = fieldName;
        View = view;
    }

    public void AddDesign(string name, View subView)
    {
        View.Add(subView);
        CreateSubControlDesign(name, subView);
    }

    public void RemoveDesign(View view)
    {
        var design = FindDesign(view, out Design? owner);

        // TODO : make this an Activity that is undoable/tracked in a 
        // central activity stack.

        if(design != null && owner != null)
        {
            owner.View.Remove(view);
            owner.subControlDesigns.Remove(design);
        }
    }

    private Design? FindDesign(View view, out Design? owner)
    {
        var found = subControlDesigns.FirstOrDefault(d => d.View == view);
        if(found != null)
        {
            owner = this;
            return found;
        }

        foreach (var subDesign in subControlDesigns)
        {
            return subDesign.FindDesign(view, out owner);
        }

        owner = null;
        return null;
    }

    public void CreateSubControlDesigns()
    {
        foreach (var subView in View.GetActualSubviews().ToArray())
        {
            logger.Info($"Found subView of Type '{subView.GetType()}'");
            
            // TODO how do we pick up the names of these fields from the source (GetType())?
            CreateSubControlDesign("unknown", subView);
        }

    }
    private void CreateSubControlDesign(string name, View subView)
    {
        subControlDesigns.Add(new Design(name, subView));

        // HACK: if you don't pull the label out first it complains that you cant set Focusable to true
        // on the Label because its super is not focusable :(
        var super = subView.SuperView;
        if(super != null)
        {
            super.Remove(subView);
        }

        // all views can be focused so that they can be edited
        // or deleted etc
        subView.CanFocus = true;

        if (super != null)
        {
            super.Add(subView);
        }

    }

    /// <summary>
    /// Gets the designable properties of the hosted View
    /// </summary>
    public virtual IEnumerable<PropertyInfo> GetProperties()
    {
        yield return View.GetType().GetProperty(nameof(View.Text));
    }


    /// <summary>
    /// Adds declaration and initialization statements to the .Designer.cs
    /// CodeDom class.
    /// </summary>
    public void ToCode(CodeTypeDeclaration addTo, CodeMemberMethod initMethod)
    {
        AddFieldToClass(addTo);
        AddConstructorCall(initMethod);

        foreach(var prop in GetProperties())
        {
            var val = prop.GetValue(View);
            AddPropertyAssignment(initMethod,prop.Name,val);
        }

        AddAddToViewStatement(initMethod);
    }

    protected void AddConstructorCall(CodeMemberMethod initMethod, params CodeExpression[] parameters)
    {
        // Construct it
        var constructLhs = new CodeFieldReferenceExpression();
        constructLhs.FieldName = $"this.{FieldName}";
        var constructRhs = new CodeObjectCreateExpression(View.GetType(),parameters);
        var constructAssign = new CodeAssignStatement();
        constructAssign.Left = constructLhs;
        constructAssign.Right = constructRhs;
        initMethod.Statements.Add(constructAssign);        
    }

    protected void AddPropertyAssignment(CodeMemberMethod initMethod, string propertyName, object? value)
    {
        var setTextLhs = new CodeFieldReferenceExpression();
        setTextLhs.FieldName = "this.myLabel.Text";
        var setTextRhs = new CodePrimitiveExpression();

        if(value is ustring u)
        {
            setTextRhs.Value = u.ToString();
        }
        else
        {
            setTextRhs.Value = value;
        }

        var setTextAssign = new CodeAssignStatement();
        setTextAssign.Left = setTextLhs;
        setTextAssign.Right = setTextRhs;
        initMethod.Statements.Add(setTextAssign);
    }

    protected void AddAddToViewStatement(CodeMemberMethod initMethod)
    {
        // Add it to the view 
        var callAdd = new CodeMethodInvokeExpression();
        callAdd.Method.TargetObject = new CodeThisReferenceExpression();
        callAdd.Method.MethodName = "Add";
        callAdd.Parameters.Add(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(),FieldName));
        initMethod.Statements.Add(callAdd);
    }

    protected void AddFieldToClass(CodeTypeDeclaration addTo)
    {
        // Create a private field for it
        var field = new CodeMemberField();
        field.Name = FieldName;
        field.Type = new CodeTypeReference(View.GetType());

        addTo.Members.Add(field);
    }
}