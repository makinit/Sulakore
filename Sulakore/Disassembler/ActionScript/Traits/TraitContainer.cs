using System.Collections.Generic;

namespace Sulakore.Disassembler.ActionScript.Traits
{
    public abstract class TraitContainer
    {
        public abstract List<ASTrait> Traits { get; }

        public SlotConstantTrait FindSlot(string name)
        {
            return FindSlot(name, "*");
        }
        public SlotConstantTrait FindSlot(string name, string typeName)
        {
            var slotTraits = FindTraits<SlotConstantTrait>(TraitType.Slot);
            foreach (SlotConstantTrait slotTrait in slotTraits)
            {
                if (name != "*" &&
                    name != slotTrait.ObjName)
                    continue;

                if (typeName != "*" &&
                    slotTrait.Type.ObjName != typeName)
                    continue;

                return slotTrait;
            }
            return null;
        }

        public SlotConstantTrait FindConstant(string name)
        {
            return FindConstant(name, "*");
        }
        public SlotConstantTrait FindConstant(string name, string typeName)
        {
            var constTraits = FindTraits<SlotConstantTrait>(TraitType.Constant);
            foreach (SlotConstantTrait constTrait in constTraits)
            {
                if (name != "*" &&
                    name != constTrait.ObjName)
                    continue;

                if (typeName != "*" &&
                    typeName != constTrait.Type.ObjName)
                    continue;

                return constTrait;
            }
            return null;
        }

        public MethodGetterSetterTrait FindMethod(string name)
        {
            return FindMethod(name, "*");
        }
        public MethodGetterSetterTrait FindMethod(string name, string returnTypeName)
        {
            var methodTraits = FindTraits<MethodGetterSetterTrait>(TraitType.Method);
            foreach (MethodGetterSetterTrait methodTrait in methodTraits)
            {
                if (name != "*" &&
                    name != methodTrait.ObjName)
                    continue;

                if (returnTypeName != "*" &&
                    returnTypeName != methodTrait.Method.ReturnType.ObjName)
                    continue;

                return methodTrait;
            }
            return null;
        }

        public MethodGetterSetterTrait FindGetter(string name)
        {
            return FindGetter(name, "*");
        }
        public MethodGetterSetterTrait FindGetter(string name, string returnTypeName)
        {
            var getterTraits = FindTraits<MethodGetterSetterTrait>(TraitType.Getter);
            foreach (MethodGetterSetterTrait getterTrait in getterTraits)
            {
                if (name != "*" &&
                    name != getterTrait.ObjName)
                    continue;

                if (returnTypeName != "*" &&
                    returnTypeName != getterTrait.Method.ReturnType.ObjName)
                    continue;

                return getterTrait;
            }
            return null;
        }

        public MethodGetterSetterTrait FindSetter(string name)
        {
            return FindSetter(name, "*");
        }
        public MethodGetterSetterTrait FindSetter(string name, string paramTypeName)
        {
            var setterTraits = FindTraits<MethodGetterSetterTrait>(TraitType.Setter);
            foreach (MethodGetterSetterTrait setterTrait in setterTraits)
            {
                if (name != "*" &&
                    name != setterTrait.ObjName)
                    continue;

                if (paramTypeName != "*" &&
                    paramTypeName != setterTrait.Method.Parameters[0].Type.ObjName)
                    continue;

                return setterTrait;
            }
            return null;
        }

        public List<T> FindTraits<T>() where T : ITrait
        {
            return FindTraits<T>((TraitType)(-1));
        }
        public List<T> FindTraits<T>(TraitType traitType) where T : ITrait
        {
            var traits = new List<T>();
            foreach (ASTrait trait in Traits)
            {
                if (!(trait.Data is T)) continue;

                if ((int)traitType == -1 ||
                    trait.TraitType == traitType)
                {
                    traits.Add((T)trait.Data);
                }
            }
            return traits;
        }
    }
}