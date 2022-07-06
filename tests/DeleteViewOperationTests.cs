﻿using NUnit.Framework;
using System.IO;
using System.Linq;
using Terminal.Gui;
using TerminalGuiDesigner;
using TerminalGuiDesigner.Operations;
using TerminalGuiDesigner.ToCode;

namespace tests
{
    internal class DeleteViewOperationTests : Tests
    {
        [Test]
        public void TestDeletingObjectWithDependency_IsImpossible()
        {
            var viewToCode = new ViewToCode();

            var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View), out var sourceCode);

            var factory = new ViewFactory();
            var lbl1 = (Label)factory.Create(typeof(Label));
            var lbl2 = (Label)factory.Create(typeof(Label));

            // add 2 labels
            new AddViewOperation(sourceCode,lbl1,designOut,"lbl1").Do();
            new AddViewOperation(sourceCode, lbl2, designOut, "lbl2").Do();

            // not impossible, we could totalyy delete either of these
            Assert.IsFalse(new DeleteViewOperation(lbl1).IsImpossible);
            Assert.IsFalse(new DeleteViewOperation(lbl2).IsImpossible);

            // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
            lbl2.X = Pos.Right(lbl1) + 5;

            Assert.IsTrue(new DeleteViewOperation(lbl1).IsImpossible);
        }

        [Test]
        public void TestDeletingObjectWithDependency_IsAllowedIfDeletingBoth()
        {
            var viewToCode = new ViewToCode();

            var file = new FileInfo("TestDeletingObjectWithDependency_IsImpossible.cs");
            var designOut = viewToCode.GenerateNewView(file, "YourNamespace", typeof(View), out var sourceCode);

            var factory = new ViewFactory();
            var lbl1 = (Label)factory.Create(typeof(Label));
            var lbl2 = (Label)factory.Create(typeof(Label));

            // add 2 labels
            new AddViewOperation(sourceCode,lbl1,designOut,"lbl1").Do();
            new AddViewOperation(sourceCode, lbl2, designOut, "lbl2").Do();

            // we now have a dependency of lbl2 on lbl1 so deleting lbl1 will go badly
            lbl2.X = Pos.Right(lbl1) + 5;

            // Deleting both at once should be possible since there are no hanging references
            Assert.IsFalse(new DeleteViewOperation(lbl1,lbl2).IsImpossible);
            Assert.IsFalse(new DeleteViewOperation(lbl2,lbl1).IsImpossible);

            Assert.AreEqual(3,designOut.GetAllDesigns().Count());
            var cmd = new DeleteViewOperation(lbl2,lbl1);
            Assert.IsTrue(cmd.Do());
            Assert.AreEqual(1,designOut.GetAllDesigns().Count());

            cmd.Undo();
            Assert.AreEqual(3,designOut.GetAllDesigns().Count());
        }
    }
}
