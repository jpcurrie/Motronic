using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace MotronicTools
{
    public class SymbolBlockCollection : SortableCollectionBase, ICustomTypeDescriptor
    {
        #region CollectionBase implementation

        public SymbolBlockCollection()
        {
            //In your collection class constructor add this line.
            //set the SortObjectType for sorting.
            base.SortObjectType = typeof(SymbolBlock);
        }

        public SymbolBlock this[int index]
        {
            get
            {
                return ((SymbolBlock)List[index]);
            }
            set
            {
                List[index] = value;
            }
        }

        public int Add(SymbolBlock value)
        {
            return (List.Add(value));

        }

        public int IndexOf(SymbolBlock value)
        {
            return (List.IndexOf(value));
        }

        public void Insert(int index, SymbolBlock value)
        {
            List.Insert(index, value);
        }

        public void Remove(SymbolBlock value)
        {
            List.Remove(value);
        }

        public bool Contains(SymbolBlock value)
        {
            // If value is not of type Int16, this will return false.
            return (List.Contains(value));
        }
        protected override void OnInsert(int index, Object value)
        {
            // Insert additional code to be run only when inserting values.
        }
        protected override void OnRemove(int index, Object value)
        {
            // Insert additional code to be run only when removing values.
        }
        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            // Insert additional code to be run only when setting values.
        }
        protected override void OnValidate(Object value)
        {
        }
        #endregion
        [TypeConverter(typeof(SymbolBlockCollectionConverter))]
        public SymbolBlockCollection Symbols
        {
            get { return this; }
        }
        internal class SymbolConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destType)
            {
                if (destType == typeof(string) && value is SymbolBlock)
                {
                    // Cast the value to an Employee type
                    SymbolBlock pp = (SymbolBlock)value;

                    return pp.Start_Address.ToString("X4") + ", " + pp.Length;
                }
                return base.ConvertTo(context, culture, value, destType);
            }
        }

        // This is a special type converter which will be associated with the EmployeeCollection class.
        // It converts an EmployeeCollection object to a string representation for use in a property grid.
        internal class SymbolBlockCollectionConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destType)
            {
                if (destType == typeof(string) && value is SymbolBlockCollection)
                {
                    // Return department and department role separated by comma.
                    return "Symbols";
                }
                return base.ConvertTo(context, culture, value, destType);
            }
        }
        #region ICustomTypeDescriptor impl

        public String GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public String GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }


        /// <summary>
        /// Called to get the properties of this type. Returns properties with certain
        /// attributes. this restriction is not implemented here.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        /// <summary>
        /// Called to get the properties of this type.
        /// </summary>
        /// <returns></returns>
        public PropertyDescriptorCollection GetProperties()
        {
            // Create a collection object to hold property descriptors
            PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

            // Iterate the list of employees
            for (int i = 0; i < this.List.Count; i++)
            {
                // Create a property descriptor for the employee item and add to the property descriptor collection
                SymbolBlockCollectionPropertyDescriptor pd = new SymbolBlockCollectionPropertyDescriptor(this, i);
                pds.Add(pd);
            }
            // return the property descriptor collection
            return pds;
        }

        #endregion
        public class SymbolBlockCollectionPropertyDescriptor : PropertyDescriptor
        {
            private SymbolBlockCollection collection = null;
            private int index = -1;

            public SymbolBlockCollectionPropertyDescriptor(SymbolBlockCollection coll, int idx)
                :
                base("#" + idx.ToString(), null)
            {
                this.collection = coll;
                this.index = idx;
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    return new AttributeCollection(null);
                }
            }

            public override bool CanResetValue(object component)
            {
                return true;
            }

            public override Type ComponentType
            {
                get
                {
                    return this.collection.GetType();
                }
            }

            public override string DisplayName
            {
                get
                {
                    SymbolBlock emp = this.collection[index];
                    return (string)(emp.Start_Address.ToString("X4"));
                }
            }

            public override string Description
            {
                get
                {
                    SymbolBlock emp = this.collection[index];
                    StringBuilder sb = new StringBuilder();
                    sb.Append(emp.Start_Address.ToString("X4"));
                    sb.Append(", ");
                    sb.Append(emp.Length);
                    return sb.ToString();
                }
            }

            public override object GetValue(object component)
            {
                return this.collection[index];
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override string Name
            {
                get { return "#" + index.ToString(); }
            }

            public override Type PropertyType
            {
                get { return this.collection[index].GetType(); }
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }

            public override void SetValue(object component, object value)
            {
                // this.collection[index] = value;
            }
        }

    }

}
