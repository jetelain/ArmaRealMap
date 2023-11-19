using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.UndoRedo;

namespace GameRealisticMap.Studio.Test.UndoRedo
{
    public class UndoableObservableCollectionTest
    {
        [Fact]
        public void AddRange()
        {
            var changes = 0;
            var collection = new UndoableObservableCollection<int>();
            collection.CollectionChanged += (_, _) => changes++;
            collection.AddRange(new[] { 1, 2, 3, 4 });
            Assert.Equal(1, changes);
            Assert.Equal(new[] { 1, 2, 3, 4 }, collection);
        }

        [Fact]
        public void RemoveRange()
        {
            var changes = 0;
            var collection = new UndoableObservableCollection<int>(new[] { 1, 2, 3, 4 });
            var list = new List<int>(new[] { 1, 2, 3, 4 });
            collection.CollectionChanged += (_, _) => changes++;
            collection.RemoveRange(2, 1);
            list.RemoveRange(2, 1);
            Assert.Equal(1, changes);
            Assert.Equal(list, collection);
            Assert.Equal(new[] { 1, 2, 4 }, collection);

            collection.RemoveRange(1, 2);
            list.RemoveRange(1, 2);
            Assert.Equal(2, changes);
            Assert.Equal(list, collection);
            Assert.Equal(new[] { 1 }, collection);
        }
    }
}
