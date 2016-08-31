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
        DynamicExtensionContainer, IEnumerable<KeyValuePair<string, object>>, INotifyPropertyChanged
    {
        /// <summary>
        /// Create a typed expando without properties
        /// </summary>
        public TypedExpando() : base((sender, arg) => ((TypedExpando)sender).RaisePropertyChanged(arg.PropertyName))
        {
            this.extension = new TypedExpandoExtension();
            this.AddExtension(extension);
        }

        private readonly TypedExpandoExtension extension;

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
        /// Remove a property from the typed expando
        /// </summary>
        /// <param name="Name">The name of the property to remove</param>
        public void RemoveProperty(string Name)
        {
            extension.RemoveProperty(Name);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return extension.MemberNames.Select(x => new KeyValuePair<string, object>(x, this[x])).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
                return extension.Get(PropertyName);
            }
            set
            {
                extension.Set(PropertyName, value);
            }
        }
    }
}
