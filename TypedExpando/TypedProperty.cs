using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypedExpando
{
    /// <summary>
    /// A PropertyDescriptor child for typed dynamic properties 
    /// </summary>
    class TypedProperty : PropertyDescriptor
    {
        public TypedProperty(Type ComponentType, string Name, IDynamicExtension Extension) : base(Name, new Attribute[0])
        {
            this.componentType = ComponentType;
            this.Extension = Extension;
        }
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
            Extension.Set(Name, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
