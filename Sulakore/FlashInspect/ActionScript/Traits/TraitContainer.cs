using System.Collections.Generic;

namespace FlashInspect.ActionScript.Traits
{
    public abstract class TraitContainer
    {
        public abstract List<ASTrait> Traits { get; }

        public ASMethod FindMethod(string methodName, string returnType)
        {
            IList<ASTrait> methodTraits = FindTraits(TraitType.Method);
            foreach (ASTrait methodTrait in methodTraits)
            {
                if (methodName != "*" && methodTrait.Name.ObjName != methodName) continue;

                var asMethod = ((MethodGetterSetterTrait)methodTrait.Data).Method;
                if (returnType != "*" && asMethod.ReturnType.ObjName != returnType) continue;

                return asMethod;
            }
            return null;
        }

        public virtual IList<ASTrait> FindTraits(TraitType traitType)
        {
            var asTraits = new List<ASTrait>();
            foreach (ASTrait trait in Traits)
            {
                if (trait.TraitType != traitType) continue;
                asTraits.Add(trait);
            }
            return asTraits;
        }
    }
}