using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicExtensions
{
    /// <summary>
    /// Proxy property getters and setters onto a collection of dynamic extensions. This class can't be instanced directly and should be inherited
    /// </summary>
    public class DynamicExtensionContainer : DynamicObject, ICustomTypeDescriptor
    {
        /// <summary>
        /// Create a new dynamic extension container without property change notification
        /// </summary>
        protected DynamicExtensionContainer() : this(null)
        {

        }

        /// <summary>
        /// Create a new dynamic extension container
        /// </summary>
        /// <param name="RaisePropertyChanged">Can be null. This callback will be called when a property is dynamicly setted</param>
        public DynamicExtensionContainer(PropertyChangedEventHandler RaisePropertyChanged)
        {
            StrongProperties = new HashSet<string>(this.GetType().GetProperties().Select(x => x.Name));
            this.raisePropertyChanged = RaisePropertyChanged;
        }

        #region DynamicObject
        private readonly PropertyChangedEventHandler raisePropertyChanged;

        /// <summary>
        /// Property names defined on the child view model
        /// </summary>
        private HashSet<string> StrongProperties;
        private List<IDynamicExtension> dynamicExtensions = new List<IDynamicExtension>();
        /// <summary>
        /// Add a dynamic extension that intercepts property gets and sets
        /// </summary>
        /// <param name="Extension"></param>
        protected void AddExtension(IDynamicExtension Extension)
        {
            dynamicExtensions.Add(Extension);
        }

        /// <summary>
        /// Get all dynamic and static property names
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return TypeDescriptor.GetProperties(this).Cast<PropertyDescriptor>().Select(x => x.Name);
        }

        /// <summary>
        /// Returns the dynamic extension that will resolve this property name. Returns null if an strong property is already named as PropertyName or if there
        /// isn't any dynamic extension that matches that property name
        /// </summary>
        /// <param name="PropertyName">The property name to search for</param>
        /// <returns></returns>
        private IDynamicExtension getDynamicExtension(string PropertyName)
        {
            if (StrongProperties.Contains(PropertyName)) return null;
            for (int i = dynamicExtensions.Count - 1; i >= 0; i--)
            {
                if (dynamicExtensions[i].MemberNames.Contains(PropertyName))
                    return dynamicExtensions[i];
            }
            return null;
        }

        /// <summary>
        /// DynamicExtensionContainer getter
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns>Returns true if the property was succesfully getted. Returns false if the property doesn't exist or if the CanRead method of the matching extensions returns false</returns>
        public sealed override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetProperty(binder.Name,  out result);
        }

        /// <summary>
        /// Returns true if a the dynamic extension succesfully solved and readed a property, else, returns falase
        /// </summary>
        /// <param name="PropertyName">The property to read</param>
        /// <param name="result">The property read result if success</param>
        /// <returns>True if success</returns>
        protected bool TryGetProperty(string PropertyName , out object result)
        {
            result = null;
            var D = getDynamicExtension(PropertyName);
            if (D == null) return false;
            if (!D.CanRead(PropertyName)) return false;

            result = D.Get(PropertyName);
            return true;
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
        /// DynamicExtensionContainer setted
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="value"></param>
        /// <returns>Returns true if the property was succesfully setted. Returns false if the property doesn't exist or if the CanWrite method of the matching extensions returns false</returns>
        public sealed override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return TrySetProperty(binder.Name, value);
        }


        /// <summary>
        /// Returns true if a the dynamic extension succesfully solved and setted a property, else, returns falase
        /// </summary>
        /// <param name="PropertyName">The property to set</param>
        /// <param name="value">The value to set</param>
        /// <returns>True if success</returns>
        protected bool TrySetProperty(string PropertyName,  object value)
        {
            var D = getDynamicExtension(PropertyName);
            //If there isn't any dynamic extension that matches that property, return false
            if (D == null) return false;
            var CanWrite = D.CanWrite(PropertyName);

            //If the property can't be written, return false
            if (!CanWrite) return false;

            //Raise the property changed only if the value was changed:
            bool RaisePropertyChanged = false;
            RaisePropertyChanged = (raisePropertyChanged != null) && D.CanRead(PropertyName) && !object.Equals(D.Get(PropertyName), value);

            //Check if the value type is compatible with the property type
            if (CanBeAssignedTo(D.GetPropertyType(PropertyName), value))
            {
                D.Set(PropertyName, value);

                if (RaisePropertyChanged)
                {
                    raisePropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
                }

                return true;
            }
            else
            {
                throw new ArgumentException("The value type isn't compatible with the property type");
            }
        }

        #region ICustomTypeDescriptor
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return new AttributeCollection();
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return GetType().Name;
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return GetType().Name;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return null;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return new EventDescriptorCollection(new EventDescriptor[0]);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return new EventDescriptorCollection(new EventDescriptor[0]);
        }


        static PropertyDescriptorCollection GetDynamicPropertyDescriptors(DynamicExtensionContainer Container)
        {
            var Result = new List<PropertyDescriptor>();

            var OriginalProperties = TypeDescriptor.GetProperties(Container, true);
            Result.AddRange(OriginalProperties.Cast<PropertyDescriptor>());

            var AllProperties = Container.dynamicExtensions.SelectMany(x => x.MemberNames).Distinct();
            foreach (var PropName in AllProperties)
            {
                var Ex = Container.getDynamicExtension(PropName);
                var PropDesc = new TypedProperty(Container.GetType(), PropName, Ex);
                Result.Add(PropDesc);
            }
            return new PropertyDescriptorCollection(Result.ToArray());
        }

        private PropertyDescriptorCollection propertyDescriptors;
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            if (propertyDescriptors == null)
                propertyDescriptors = GetDynamicPropertyDescriptors(this);
            return propertyDescriptors;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }


        #endregion
        #endregion

    }
}
