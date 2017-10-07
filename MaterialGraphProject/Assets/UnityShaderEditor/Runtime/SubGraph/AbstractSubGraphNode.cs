﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Graphing;

namespace UnityEngine.MaterialGraph
{
    public abstract class AbstractSubGraphNode : AbstractMaterialNode
        , IGeneratesFunction
        , IOnAssetEnabled
        , IMayRequireNormal
        , IMayRequireTangent
        , IMayRequireBitangent
        , IMayRequireMeshUV
        , IMayRequireScreenPosition
        , IMayRequireViewDirection
        , IMayRequirePosition
        , IMayRequireVertexColor
        , IMayRequireTime
    {

        protected virtual AbstractSubGraph referencedGraph { get; }

        public override bool hasPreview
        {
            get { return referencedGraph != null; }
        }

        public override PreviewMode previewMode
        {
            get
            {
                if (referencedGraph == null)
                    return PreviewMode.Preview2D;

                return PreviewMode.Preview3D;
            }
        }

        public virtual INode outputNode
        {
            get { return null; }
        }

        public virtual void OnEnable()
        {
            var validNames = new List<int>();
            if (referencedGraph == null)
            {
                RemoveSlotsNameNotMatching(validNames);
                return;
            }

            var props = referencedGraph.properties;
            foreach (var prop in props)
            {
                var propType = prop.propertyType;
                SlotValueType slotType;

                switch (propType)
                {
                    case PropertyType.Color:
                        slotType = SlotValueType.Vector4;
                        break;
                    case PropertyType.Texture:
                        slotType = SlotValueType.Texture2D;
                        break;
                    case PropertyType.Float:
                        slotType = SlotValueType.Vector1;
                        break;
                    case PropertyType.Vector2:
                        slotType = SlotValueType.Vector2;
                        break;
                    case PropertyType.Vector3:
                        slotType = SlotValueType.Vector3;
                        break;
                    case PropertyType.Vector4:
                        slotType = SlotValueType.Vector4;
                        break;
                    case PropertyType.Matrix2:
                        slotType = SlotValueType.Matrix2;
                        break;
                    case PropertyType.Matrix3:
                        slotType = SlotValueType.Matrix3;
                        break;
                    case PropertyType.Matrix4:
                        slotType = SlotValueType.Matrix4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var id = prop.guid.GetHashCode();
                AddSlot(new MaterialSlot(id, prop.displayName, prop.referenceName, SlotType.Input, slotType, prop.defaultValue));
                validNames.Add(id);
            }

            var subGraphOutputNode = outputNode;
            if (outputNode != null)
            {
                foreach (var slot in subGraphOutputNode.GetInputSlots<MaterialSlot>())
                {
                    AddSlot(new MaterialSlot(slot.id, slot.displayName, slot.shaderOutputName, SlotType.Output, slot.valueType, slot.defaultValue));
                    validNames.Add(slot.id);
                }
            }

            RemoveSlotsNameNotMatching(validNames);
        }

        public override void CollectShaderProperties(PropertyCollector visitor, GenerationMode generationMode)
        {
            base.CollectShaderProperties(visitor, generationMode);

            if (referencedGraph == null)
                return;

            referencedGraph.CollectShaderProperties(visitor, GenerationMode.ForReals);
        }

        public override void CollectPreviewMaterialProperties(List<PreviewProperty> properties)
        {
            base.CollectPreviewMaterialProperties(properties);

            if (referencedGraph == null)
                return;

            properties.AddRange(referencedGraph.GetPreviewProperties());
        }

        public void GenerateNodeFunction(ShaderGenerator visitor, GenerationMode generationMode)
        {
            if (referencedGraph == null)
                return;

            referencedGraph.GenerateNodeFunction(visitor, GenerationMode.ForReals);
        }

        public NeededCoordinateSpace RequiresNormal()
        {
            if (referencedGraph == null)
                return NeededCoordinateSpace.None;

            return referencedGraph.activeNodes.OfType<IMayRequireNormal>().Aggregate(NeededCoordinateSpace.None, (mask, node) =>
            {
                mask |= node.RequiresNormal();
                return mask;
            });
        }

        public bool RequiresMeshUV(UVChannel channel)
        {
            if (referencedGraph == null)
                return false;

            return referencedGraph.activeNodes.OfType<IMayRequireMeshUV>().Any(x => x.RequiresMeshUV(channel));
        }

        public bool RequiresScreenPosition()
        {
            if (referencedGraph == null)
                return false;

            return referencedGraph.activeNodes.OfType<IMayRequireScreenPosition>().Any(x => x.RequiresScreenPosition());
        }

        public NeededCoordinateSpace RequiresViewDirection()
        {
            if (referencedGraph == null)
                return NeededCoordinateSpace.None;

            return referencedGraph.activeNodes.OfType<IMayRequireViewDirection>().Aggregate(NeededCoordinateSpace.None, (mask, node) =>
            {
                mask |= node.RequiresViewDirection();
                return mask;
            });
        }


        public NeededCoordinateSpace RequiresPosition()
        {
            if (referencedGraph == null)
                return NeededCoordinateSpace.None;

            return referencedGraph.activeNodes.OfType<IMayRequirePosition>().Aggregate(NeededCoordinateSpace.None, (mask, node) =>
            {
                mask |= node.RequiresPosition();
                return mask;
            });
        }

        public NeededCoordinateSpace RequiresTangent()
        {
            if (referencedGraph == null)
                return NeededCoordinateSpace.None;

            return referencedGraph.activeNodes.OfType<IMayRequireTangent>().Aggregate(NeededCoordinateSpace.None, (mask, node) =>
            {
                mask |= node.RequiresTangent();
                return mask;
            });
        }

        public bool RequiresTime()
        {
            if (referencedGraph == null)
                return false;

            return referencedGraph.activeNodes.OfType<IMayRequireTime>().Any(x => x.RequiresTime());
        }

        public NeededCoordinateSpace RequiresBitangent()
        {
            if (referencedGraph == null)
                return NeededCoordinateSpace.None;

            return referencedGraph.activeNodes.OfType<IMayRequireBitangent>().Aggregate(NeededCoordinateSpace.None, (mask, node) =>
            {
                mask |= node.RequiresBitangent();
                return mask;
            });
        }

        public bool RequiresVertexColor()
        {
            if (referencedGraph == null)
                return false;

            return referencedGraph.activeNodes.OfType<IMayRequireVertexColor>().Any(x => x.RequiresVertexColor());
        }
    }
}
