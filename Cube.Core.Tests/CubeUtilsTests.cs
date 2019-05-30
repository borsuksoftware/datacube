using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.Cube
{
    #region DoubleValue Class
    
    public class DoubleValue : ICubeValue
    {
        public double Value { get; set; }
        
        public DoubleValue(double value)
        {
            this.Value = value;
        }
        
        public ICubeValue Clone()
        {
            return new DoubleValue(this.Value);
        }

        public void Add(ICubeValue cubeValue)
        {
            var otherValue = cubeValue as DoubleValue;
            if( otherValue == null )
                throw new InvalidOperationException( $"Unable to add value of type '{cubeValue.GetType()}'");

            this.Value += otherValue.Value;
        }
    }
    
    #endregion
    
    [TestClass]
    public class CubeUtilsTests
    {
        [TestMethod]
        public void CombineCubes_Simple()
        {
            (var cube1, var cube2) = CubePoints();

            var combinedCube = CubeUtils.CombineCubes(new[] {cube1, cube2});
            Assert.AreEqual(2, combinedCube.AxisSet.Count);
            Assert.IsTrue(combinedCube.AxisSet.Any( axis => axis.Name == "str1"));
            Assert.IsTrue(combinedCube.AxisSet.Any( axis => axis.Name == "str3"));
            
            Assert.AreEqual(1, combinedCube.Count());

            var val = combinedCube.Single();
            Assert.AreEqual(11,val.CubeValue.Value );

        }

        [TestMethod]
        public void CombineCubes_Axes_Order1()
        {
            (var cube1, var cube2) = CubePoints();

            var combinedCube = CubeUtils.CombineCubes(new[] {cube1, cube2}, new string[] { "str1", "str3"});
            Assert.AreEqual(2, combinedCube.AxisSet.Count);
            Assert.IsTrue(combinedCube.AxisSet.Any( axis => axis.Name == "str1"));
            Assert.IsTrue(combinedCube.AxisSet.Any( axis => axis.Name == "str3"));

            var axisNames = combinedCube.AxisSet.Select(a => a.Name).ToList();
            Assert.AreEqual("str1", axisNames[0]);
            Assert.AreEqual("str3", axisNames[1]);
            
            Assert.AreEqual(1, combinedCube.Count());

            var val = combinedCube.Single();
            Assert.AreEqual(11,val.CubeValue.Value );
        }

        [TestMethod]
        public void CombineCubes_Axes_Order2()
        {
            (var cube1, var cube2) = CubePoints();

            var combinedCube = CubeUtils.CombineCubes(new[] {cube1, cube2}, new string[] {"str3", "str1"});
            Assert.AreEqual(2, combinedCube.AxisSet.Count);
            Assert.IsTrue(combinedCube.AxisSet.Any(axis => axis.Name == "str1"));
            Assert.IsTrue(combinedCube.AxisSet.Any(axis => axis.Name == "str3"));

            var axisNames = combinedCube.AxisSet.Select(a => a.Name).ToList();
            Assert.AreEqual("str3", axisNames[0]);
            Assert.AreEqual("str1", axisNames[1]);

            Assert.AreEqual(1, combinedCube.Count());

            var val = combinedCube.Single();
            Assert.AreEqual(11, val.CubeValue.Value);
        }
        
        [TestMethod]
        public void CombineCubes_Axes_Subset()
        {
            (var cube1, var cube2) = CubePoints();

            var combinedCube = CubeUtils.CombineCubes(new[] {cube1, cube2}, new string[] {"str3"});
            Assert.AreEqual(1, combinedCube.AxisSet.Count);
            Assert.IsTrue(combinedCube.AxisSet.Any(axis => axis.Name == "str3"));

            Assert.AreEqual(1, combinedCube.Count());

            var val = combinedCube.Single();
            Assert.AreEqual(11, val.CubeValue.Value);
        }

        private static Tuple<Cube<DoubleValue>,Cube<DoubleValue>> CubePoints()
        {
            var cube1 = new Cube<DoubleValue>(
                new AxisSet(new[]
                {
                    new Axis("str1", typeof(string), true),
                    new Axis("str2", typeof(string), true),
                    new Axis("str3", typeof(string), true),
                }));

            var cube2 = new Cube<DoubleValue>(
                new AxisSet(new[]
                {
                    new Axis("str1", typeof(string), true),
                    new Axis("str3", typeof(string), true),
                }));

            {
                var keys1 = new Dictionary<string, object>();
                keys1["str1"] = "a1";
                keys1["str2"] = "a2";
                keys1["str3"] = "a3";
                cube1.AddItem(keys1, new DoubleValue(5));
            }
            {
                var keys1 = new Dictionary<string, object>();
                keys1["str1"] = "a1";
                keys1["str2"] = "a2a";
                keys1["str3"] = "a3";
                cube1.AddItem(keys1, new DoubleValue(6));
            }
            
            return Tuple.Create(cube1,cube2);
        }
    }
}