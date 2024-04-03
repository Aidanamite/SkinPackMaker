using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SimpleResourceReplacer
{
    [Serializable]
    public abstract class CustomDragonEquipment
    {
        public string Name;
        public int ItemID;
        public string SkinIcon;
        public int PetType;
        [OptionalField]
        public string RequiredAge = "TEEN";
    }

    [Serializable]
    public class CustomSaddle : CustomDragonEquipment
    {
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public bool CustomMesh;
        public string Mesh;
        public string Texture;
    }
    [Serializable]
    public class CustomSkin : CustomDragonEquipment
    {
        public string[] TargetRenderers;
        public MaterialProperty[] MaterialData;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public MeshOverrides Mesh;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public MaterialProperty[] HWMaterialData;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public Shaders BabyShaders;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public Shaders TeenShaders;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public Shaders AdultShaders;
        [OptionalField]
        [DataMember(EmitDefaultValue = false)]
        public Shaders TitanShaders;
        [Serializable]
        public class Shaders
        {
            [OptionalField]
            public string Body;
            [OptionalField]
            public string Eyes;
            [OptionalField]
            public string Extra;
        }
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
