using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DynamicExtensions
{
    /// <summary>
    /// A dynamic class that supports adding typed properties in runtime
    /// </summary>
    public class TypedExpando :
        DynamicExtensionContainer, IEnumerable<KeyValuePair<string, object>>, INotifyPropertyChanged, IDictionary<string, object>
    {

        /// <summary>
        /// Create a typed expando without properties
        /// </summary>
        public TypedExpando() : this(new Tuple<string, Type>[0])
        {
        }

        /// <summary>
        /// Create a typed expando with the given properties
        /// </summary>
        /// <param name="Properties">A collection of pairs of property name and property type</param>
        public TypedExpando(IEnumerable<Tuple<string, Type>> Properties) : base((sender, arg) => ((TypedExpando)sender).RaisePropertyChanged(arg.PropertyName))
        {
            this.extension = new TypedExpandoExtension();
            this.AddExtension(extension);

            //Add properties:
            foreach (var P in Properties)
                AddProperty(P.Item1, P.Item2);

            this.Keys = extension.Properties.Keys;
            this.Values = ((IDictionary<string, object>)extension.Properties).Values;
        }

        private readonly TypedExpandoExtension extension;

        /// <summary>
        /// 
        /// </summary>
        public ICollection<string> Keys
        {
            get; private set;
        }

        /// <summary>
        /// Get the collection of all properties values
        /// </summary>
        public ICollection<object> Values { get; private set; }

        /// <summary>
        /// Gets the number of properties of this typed expando
        /// </summary>
        public int Count
        {
            get
            {
                return extension.Properties.Count;
            }
        }
        /// <summary>
        /// Returns false
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Informs when a dynamic property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise the PropertyChanged event
        /// </summary>
        /// <param name="PropertyName">The property name. If not specified is taken from the caller member name</param>
        protected void RaisePropertyChanged([CallerMemberName] string PropertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        /// <summary>
        /// Add a new property to the typed expando
        /// </summary>
        /// <param name="Name">Property name</param>
        /// <param name="Type">Property type</param>
        public void AddProperty(string Name, Type Type)
        {
            extension.AddProperty(Name, Type);
        }


        /// <summary>
        /// Gets the default enumerator
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return extension.MemberNames.Select(x => new KeyValuePair<string, object>(x, this[x])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns true if the current object contains the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return this.extension.Properties.ContainsKey(key);
        }

        /// <summary>
        /// Add a property with a given value. The property type will be value.GetType(), if value is null, the type will be object.
        /// Consider using the method AddProperty for explicitly specifing property type
        /// </summary>
        /// <param name="key">Property name</param>
        /// <param name="value">Property initial value and from the property type will be infered</param>
        public void Add(string key, object value)
        {
            var Type = value == null ? typeof(object) : value.GetType();
            AddProperty(key, Type);
            this[key] = value;
        }

        /// <summary>
        /// Remove a property from the typed expando. If the 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key))
            {
                extension.RemoveProperty(key);
                return true;
            }
            else
                return false;

        }

        /// <summary>
        /// Try to get a property value
        /// </summary>
        /// <param name="key">The property name</param>
        /// <param name="value">Result</param>
        /// <returns>True if the property was found</returns>
        public bool TryGetValue(string key, out object value)
        {
            return TryGetProperty(key, out value);
        }

        /// <summary>
        /// Add a property with a given value. The property type will be value.GetType(), if value is null, the type will be object.
        /// Consider using the method AddProperty for explicitly specifing property type
        /// </summary>
        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <summary>
        /// Remove all properties from the object
        /// </summary>
        public void Clear()
        {
            this.extension.Properties.Clear();
        }

        /// <summary>
        /// Returns true if the object contains the given property name and value pair
        /// </summary>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return this.extension.Properties.Any(x => x.Key == item.Key && x.Value.Value == item.Value);
        }

        /// <summary>
        /// Copy all property name and value pairs to an array
        /// </summary>
        /// <param name="array">The destination array</param>
        /// <param name="arrayIndex">The initial array index</param>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach (var KV in this)
            {
                array[i] = new KeyValuePair<string, object>(KV.Key, KV.Value);
                i++;
            }
        }

        /// <summary>
        /// Remove a given property name and value pair
        /// </summary>
        /// <param name="item">The item to delete</param>
        /// <returns>True if the pair was found, else false</returns>
        public bool Remove(KeyValuePair<string, object> item)
        {
            if (!ContainsKey(item.Key))
                return false;
            if (!object.Equals(this[item.Key], item.Value))
                return false;

            return this.Remove(item.Key);
        }

        /// <summary>
        /// Gets or sets a property by its name
        /// </summary>
        /// <param name="PropertyName">Property name</param>
        /// <returns></returns>
        public object this[string PropertyName]
        {
            get
            {
                object result;
                if (TryGetProperty(PropertyName, out result))
                {
                    return result;
                }
                else
                    throw new ArgumentException($"Property '{PropertyName}' not found");
            }
            set
            {
                if (!TrySetProperty(PropertyName, value))
                    throw new ArgumentException($"Property '{PropertyName}' not found");
            }
        }
    }
}
