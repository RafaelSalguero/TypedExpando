using System;
using System.Collections.Generic;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.ComponentModel;

namespace DynamicExtensions.Test
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void PropertyChangedAccesorTest()
        {
            var O = new TypedExpando(new[] {
                Tuple.Create ( "Age", typeof(int)),
                Tuple.Create ("Name", typeof(string) )
                }
            );

            bool Ok = false;
            O.PropertyChanged += (sender, e) =>
            {
                Assert.AreEqual("Name", e.PropertyName);
                Ok = true;
            };
            var D = (dynamic)O;

            var CTD = (ICustomTypeDescriptor)O;
            CTD.GetProperties().Cast<PropertyDescriptor>().First(x => x.Name == "Name").SetValue(CTD, "Rafael");

            Assert.IsTrue(Ok);
        }

        [TestMethod]
        public void PropertyChangedTest()
        {
            var O = new TypedExpando(new[] {
                Tuple.Create ( "Age", typeof(int)),
                Tuple.Create ("Name", typeof(string) )
                }
            );

            bool Ok = false;
            O.PropertyChanged += (sender, e) =>
             {
                 Assert.AreEqual("Name", e.PropertyName);
                 Ok = true;
             };
            var D = (dynamic)O;

            D.Name = "Rafa";

            Assert.IsTrue(Ok);
        }

        [TestMethod]
        public void PropertyChangedTestIndexer()
        {
            var O = new TypedExpando(new[] {
                Tuple.Create ( "Age", typeof(int)),
                Tuple.Create ("Name", typeof(string) )
                }
            );

            bool Ok = false;
            O.PropertyChanged += (sender, e) =>
            {
                Assert.AreEqual("Name", e.PropertyName);
                Ok = true;
            };
            var D = (dynamic)O;

            D["Name"] = "Rafa";

            Assert.IsTrue(Ok);
        }


        [TestMethod]
        public void GetSetTest()
        {
            var O = new TypedExpando();
            O.AddProperty("Name", typeof(string));
            O.AddProperty("Age", typeof(int));

            var D = (dynamic)O;

            D.Name = "Rafa";
            D.Age = 22;

            Assert.AreEqual("Rafa", D.Name);
            Assert.AreEqual(22, D.Age);
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void PropertyDoesNotExistsTest()
        {
            var O = new TypedExpando();
            O.AddProperty("Name", typeof(string));

            var D = (dynamic)O;
            D.Age = 22;
        }

        [TestMethod]
        public void PropertyTypeMatchTest()
        {
            var O = new TypedExpando();
            O.AddProperty("Numbers", typeof(IReadOnlyList<int>));

            var D = (dynamic)O;
            D.Numbers = new[] { 1, 2, 3 };

            Assert.IsTrue(((IEnumerable<int>)D.Numbers).SequenceEqual(new[] { 1, 2, 3 }));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PropertyTypeMismatchTest()
        {
            var O = new TypedExpando();
            O.AddProperty("Age", typeof(int));

            var D = (dynamic)O;
            D.Age = "Hello";
        }

        [TestMethod]
        public void DefaultValueTest()
        {
            var O = new TypedExpando();
            O.AddProperty("Age", typeof(int));

            var D = (dynamic)O;
            Assert.AreEqual(0, D.Age);
        }

        [TestMethod]
        public void NullableValueTest()
        {
            var O = new TypedExpando();
            O.AddProperty("Age", typeof(int?));

            //Initial value should be null
            var D = (dynamic)O;
            Assert.AreEqual(null, D.Age);

            D.Age = 20;
            Assert.AreEqual(20, D.Age);

            D.Age = null;
            Assert.AreEqual(null, D.Age);
        }


    }
}
