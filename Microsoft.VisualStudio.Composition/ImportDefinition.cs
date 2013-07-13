﻿namespace Microsoft.VisualStudio.Composition
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Validation;

    [DebuggerDisplay("{Contract.Type.Name,nq} (Lazy: {IsLazy}, {Cardinality})")]
    public class ImportDefinition : IEquatable<ImportDefinition>
    {
        private readonly Type wrapperType;

        public ImportDefinition(CompositionContract contract, ImportCardinality cardinality, Type wrapperType, IReadOnlyCollection<IImportSatisfiabilityConstraint> additionalConstraints)
        {
            Requires.NotNull(contract, "contract");
            Requires.NotNull(additionalConstraints, "additionalConstraints");

            this.Contract = contract;
            this.Cardinality = cardinality;
            this.wrapperType = wrapperType;
            this.ExportContraints = additionalConstraints;
        }

        public ImportCardinality Cardinality { get; private set; }

        public bool IsLazy
        {
            get { return this.wrapperType.IsAnyLazyType(); }
        }

        public bool IsLazyConcreteType
        {
            get { return this.wrapperType.IsConcreteLazyType(); }
        }

        public Type LazyType
        {
            get { return this.IsLazy ? this.wrapperType : null; }
        }

        public bool IsExportFactory
        {
            get { return this.wrapperType.IsExportFactoryTypeV1() || this.wrapperType.IsExportFactoryTypeV2(); }
        }

        public Type ExportFactoryType
        {
            get { return this.IsExportFactory ? this.wrapperType : null; }
        }

        public Type MetadataType
        {
            get
            {
                if (this.LazyType != null)
                {
                    var lazyTypeDefinition = this.LazyType.GetGenericTypeDefinition();
                    if (lazyTypeDefinition.IsEquivalentTo(typeof(Lazy<,>)) || lazyTypeDefinition.IsEquivalentTo(typeof(ILazy<,>)))
                    {
                        return this.LazyType.GetGenericArguments()[1];
                    }
                }

                return null;
            }
        }

        public CompositionContract Contract { get; private set; }

        public IReadOnlyCollection<IImportSatisfiabilityConstraint> ExportContraints { get; private set; }

        /// <summary>
        /// Gets the actual type (without the Lazy{T} wrapper) of the importing member.
        /// </summary>
        public Type CoercedValueType
        {
            get
            {
                // MEF v2 only allows for this to match the contract itself. MEF v1 was more flexible.
                return this.Contract.Type;
            }
        }

        public override int GetHashCode()
        {
            return this.Contract.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as ImportDefinition);
        }

        public bool Equals(ImportDefinition other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Contract.Equals(other.Contract)
                && this.Cardinality == other.Cardinality;
        }
    }
}
