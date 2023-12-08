using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;

namespace GameRealisticMap.Studio.Test.Modules.AssetConfigEditor.ViewModels.Filling
{

    public class FillingAssetClusterViewModelTest
    {

        [Fact]
        public void IsEmpty()
        {
            var vm = new FillingAssetClusterViewModel(Arma3.Assets.Filling.ClusterCollectionId.Forest, null, AssetConfigHelper.CreateEditor());

            Assert.True(vm.IsEmpty); // No seed

            vm.AddEmptySeed.Execute(null);

            Assert.True(vm.IsEmpty); // One empty seed

            AssetConfigHelper.AddSomeObject(vm.Items[0]);

            Assert.False(vm.IsEmpty); // Seed is no more empty
        }

        [Fact]
        public void ToDefinition()
        {
            var vm = new FillingAssetClusterViewModel(Arma3.Assets.Filling.ClusterCollectionId.Forest, null, AssetConfigHelper.CreateEditor());

            vm.Probability = 0.5;
            vm.Density.MinDensity = 0.01;
            vm.Density.MaxDensity = 0.1;
            vm.Label = "Label";

            var definition = vm.ToDefinition();
            Assert.Equal(0.5, definition.Probability);
            Assert.Equal(0.01, definition.MinDensity);
            Assert.Equal(0.1, definition.MaxDensity);
            Assert.Equal("Label", definition.Label);
            Assert.Empty(definition.Clusters);

            // Add an empty seed
            vm.AddEmptySeed.Execute(null);

            definition = vm.ToDefinition();
            Assert.Empty(definition.Clusters);

            // Add an object into seed
            AssetConfigHelper.AddSomeObject(vm.Items[0]);

            definition = vm.ToDefinition();
            var seed = Assert.Single(definition.Clusters);
            Assert.Equal(1, seed.Probability);

            var item = Assert.Single(seed.Models);
            Assert.Equal(1, item.Probability);
            Assert.Equal("SomeModel", Assert.Single(item.Model.Objects).Model.Name);

            // Add an empty seed
            vm.AddEmptySeed.Execute(null);

            definition = vm.ToDefinition();
            seed = Assert.Single(definition.Clusters);
            Assert.Equal(1, seed.Probability);

            // Add an object into the second seed
            AssetConfigHelper.AddSomeObject(vm.Items[1]);
            vm.MakeItemsEquiprobable(); // reset probability as first item was changed to 1

            definition = vm.ToDefinition();
            Assert.Equal(0.5, definition.Clusters[0].Probability);
            Assert.Equal(0.5, definition.Clusters[1].Probability);
        }
    }
}
