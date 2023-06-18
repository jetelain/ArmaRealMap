using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Caliburn.Micro;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.UndoRedo
{
    internal static class UndoRedoExtensions
    {
        public static void ChangeProperty<TObject,TValue>(this IUndoRedoManager undoRedoManager, TObject obj, Expression<Func<TObject,TValue>> expression, TValue newValue)
        {
            var member = (PropertyInfo)expression.GetMemberInfo();
            var oldValue = (TValue)member.GetValue(obj)!;
            if (!EqualityComparer<TValue>.Default.Equals(oldValue, newValue))
            {
                undoRedoManager.ExecuteAction(new PropertyAction<TValue>(newValue, oldValue, member.Name, value => member.SetValue(obj, value)));
            }
        }
    }
}
