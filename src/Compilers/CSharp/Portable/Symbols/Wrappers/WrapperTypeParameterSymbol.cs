﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using Roslyn.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    /// <summary>
    /// Represents a type parameter that is based on another type parameter.
    /// When inheriting from this class, one shouldn't assume that 
    /// the default behavior it has is appropriate for every case.
    /// That behavior should be carefully reviewed and derived type
    /// should override behavior as appropriate.
    /// </summary>
    internal abstract class WrapperTypeParameterSymbol
        : TypeParameterSymbol
    {
        /// <summary>
        /// The underlying TypeParameterSymbol, cannot be another RetargetingTypeParameterSymbol.
        /// </summary>
        protected readonly TypeParameterSymbol _underlyingTypeParameter;

        public WrapperTypeParameterSymbol(TypeParameterSymbol underlyingTypeParameter)
        {
            Debug.Assert((object)underlyingTypeParameter != null);
            _underlyingTypeParameter = underlyingTypeParameter;
        }

        public TypeParameterSymbol UnderlyingTypeParameter
        {
            get
            {
                return _underlyingTypeParameter;
            }
        }

        public override bool IsImplicitlyDeclared
        {
            get { return _underlyingTypeParameter.IsImplicitlyDeclared; }
        }

        public override TypeParameterKind TypeParameterKind
        {
            get
            {
                return _underlyingTypeParameter.TypeParameterKind;
            }
        }

        public override int Ordinal
        {
            get
            {
                return _underlyingTypeParameter.Ordinal;
            }
        }

        public override bool HasConstructorConstraint
        {
            get
            {
                return _underlyingTypeParameter.HasConstructorConstraint;
            }
        }

        public override bool HasReferenceTypeConstraint
        {
            get
            {
                return _underlyingTypeParameter.HasReferenceTypeConstraint;
            }
        }

        public override bool HasValueTypeConstraint
        {
            get
            {
                return _underlyingTypeParameter.HasValueTypeConstraint;
            }
        }

        public override VarianceKind Variance
        {
            get
            {
                return _underlyingTypeParameter.Variance;
            }
        }

        public override abstract Symbol ContainingSymbol
        {
            get;
        }

        public override ImmutableArray<Location> Locations
        {
            get
            {
                return _underlyingTypeParameter.Locations;
            }
        }

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
        {
            get
            {
                return _underlyingTypeParameter.DeclaringSyntaxReferences;
            }
        }

        public override abstract ImmutableArray<CSharpAttributeData> GetAttributes();

        public override string Name
        {
            get
            {
                return _underlyingTypeParameter.Name;
            }
        }

        public override string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _underlyingTypeParameter.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
        }

        internal override void EnsureAllConstraintsAreResolved()
        {
            _underlyingTypeParameter.EnsureAllConstraintsAreResolved();
        }

        internal override abstract ImmutableArray<TypeSymbol> GetConstraintTypes(ConsList<TypeParameterSymbol> inProgress);

        internal override abstract ImmutableArray<NamedTypeSymbol> GetInterfaces(ConsList<TypeParameterSymbol> inProgress);

        internal override abstract NamedTypeSymbol GetEffectiveBaseClass(ConsList<TypeParameterSymbol> inProgress);

        internal override abstract TypeSymbol GetDeducedBaseType(ConsList<TypeParameterSymbol> inProgress);
    }
}
