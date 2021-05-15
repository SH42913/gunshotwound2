using System;
using System.Globalization;
using System.Windows.Forms;
using System.Xml.Linq;
using Leopotam.Ecs;

namespace GunshotWound2.Utils
{
    public static class VariousExtensions
    {
        public static float NextFloat(this Random rand, float min, float max)
        {
            if (min > max)
            {
                throw new ArgumentException("min must be less than max");
            }

            return (float) rand.NextDouble() * (max - min) + min;
        }

        public static void CleanFilter(this EcsFilter filter)
        {
            var world = filter.GetWorld();
            for (var i = 0; i < filter.EntitiesCount; i++)
            {
                world.RemoveEntity(filter.Entities[i]);
            }
        }

        public static bool GetBool(this XElement node, string attributeName = "Value")
        {
            var value = string.IsNullOrEmpty(attributeName)
                ? node.Value
                : node.Attribute(attributeName).Value;

            return !string.IsNullOrEmpty(value) && bool.Parse(value);
        }

        public static int GetInt(this XElement node, string attributeName = "Value")
        {
            var value = string.IsNullOrEmpty(attributeName)
                ? node.Value
                : node.Attribute(attributeName).Value;

            return int.Parse(value);
        }

        public static float GetFloat(this XElement node, string attributeName = "Value")
        {
            var value = string.IsNullOrEmpty(attributeName)
                ? node.Value
                : node.Attribute(attributeName).Value;

            return float.Parse(value, CultureInfo.InvariantCulture);
        }

        public static Keys? GetKey(this XElement node, string name)
        {
            if (string.IsNullOrEmpty(node?.Element(name)?.Value)) return null;

            return (Keys) int.Parse(node.Element(name).Value);
        }
    }
}