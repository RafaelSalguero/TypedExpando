using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicExtensions
{
    /// <summary>
    /// A PropertyDescriptor child for typed dynamic properties 
    /// </summary>
    class TypedProperty : PropertyDescriptor
    {
        public TypedProperty(Type ComponentType, string Name, IDynamicExtension Extension, PropertyChangedEventHandler raisePropertyChanged) : base(Name, new Attribute[0])
        {
            this.componentType = ComponentType;
            this.Extension = Extension;
            this.raisePropertyChanged = raisePropertyChanged;
        }
        readonly PropertyChangedEventHandler raisePropertyChanged;
        readonly IDynamicExtension Extension;
        readonly Type componentType;

        public override Type ComponentType
        {
            get
            {
                return ComponentType;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return !Extension.CanWrite(Name);
            }
        }

        public override Type PropertyType
        {
            get
            {
                return Extension.GetPropertyType(Name);
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            return Extension.Get(Name);
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            SetValue(this.PropertyType, this.Name, value, component, this.raisePropertyChanged, true, () => this.GetValue(component), () => Extension.Set(Name, value));
        }

        /// <summary>
        /// Check if a given value can be assigned to a property of a given type
        /// </summary>
        static bool CanBeAssignedTo(Type Type, object Value)
        {
            //If type is nullable, return true if value is null
            if (Value == null)
            {
                //If the type is nullable:
                if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return true;
                if (Type.IsValueType)
                    return false;
                else
                    return true;
            }
            else
            {
                return Type.IsInstanceOfType(Value);
            }
        }

        /// <summary>
        /// Apply all necesary logic to set a property value
        /// </summary>
        /// <param name="PropertyType">Property type that will be used to check type compatibility</param>
        /// <param name="PropertyName">Property name used for change notification</param>
        /// <param name="value">Value to set</param>
        /// <param name="sender">Sender argument used to change notification</param>
        /// <param name="raisePropertyChanged">Event handler, can be null is change notification is not needed</param>
        /// <param name="CanRead">True if this property can be readed</param>
        /// <param name="GetValue">Getter function, used by change notification to compare new and old values</param>
        /// <param name="SetValue">Setter function</param>
        public static void SetValue(Type PropertyType, string PropertyName, object value, object sender, PropertyChangedEventHandler raisePropertyChanged, bool CanRead, Func<object> GetValue, Action SetValue)
        {
            //Raise the property changed only if the value was changed:
            bool RaisePropertyChanged = false;
            RaisePropertyChanged = (raisePropertyChanged != null) && CanRead && !object.Equals(GetValue(), value);

            //Check if the value type is compatible with the property type
            if (CanBeAssignedTo(PropertyType, value))
            {
                SetValue();

                if (RaisePropertyChanged)
                {
                    raisePropertyChanged(sender, new PropertyChangedEventArgs(PropertyName));
                }
            }
            else
            {
                throw new ArgumentException("The value type isn't compatible with the property type");
            }
        }


        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
