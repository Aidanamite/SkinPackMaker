using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SimpleResourceReplacer
{
    [Serializable]
    public class CustomSkin
    {
        public string Name;
        public int ItemID;
        public string SkinIcon;
        public string[] TargetRenderers;
        public MaterialProperty[] MaterialData;
        public int PetType;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public MeshOverrides Mesh;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public MaterialProperty[] HWMaterialData;
    }
    [Serializable]
    public class MaterialProperty
    {
        public string Property;
        public string Value;
        public string Target;
    }
    [Serializable]
    public class MeshOverrides
    {
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public string Baby;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public string Teen;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public string Adult;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public string Titan;
    }
}
