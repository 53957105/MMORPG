﻿using Common.Proto.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Tool
{
    public static class VectorHelper
    {
        public static Vector3 ToEulerAngles(this Vector3 direction)
        {
            float Rad2Deg = 57.29578f;
            var eulerAngles = new Vector3();

            // Anglex = arc cos(sqrt((x^2 + z^2) / (x^2 + y^2 + z^2)))
            eulerAngles.X = MathF.Acos(
                MathF.Sqrt(
                    (direction.X * direction.X + direction.Z * direction.Z) /
                    (direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z)
                )
                * Rad2Deg
            );
            if (direction.Y > 0) eulerAngles.X = 360 - eulerAngles.X;

            // AngleY = arc tan(x/z)
            eulerAngles.Y = MathF.Atan2(direction.X, direction.Z) * Rad2Deg;
            if (eulerAngles.Y < 0) eulerAngles.Y += 180;
            if (direction.X < 0) eulerAngles.Y += 180;

            // AngleZ = 0
            eulerAngles.Z = 0;
            return eulerAngles;
        }

        public static Vector3 Normalize(this Vector3 value)
        {
            float num = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
            if (num > 1E-05f)
            {
                return value / num;
            }
            return Vector3.Zero;
        }

        public static Vector2 ToVector2(this Vector3 vector3)
        {
            var vector2 = new Vector2(vector3.X, vector3.Z);
            return vector2;
        }
    }
}