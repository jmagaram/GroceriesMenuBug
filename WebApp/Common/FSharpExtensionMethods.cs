﻿using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Common {
    public static class OptionExtensions {
        public static bool IsSome<T>(this FSharpOption<T> o) => FSharpOption<T>.get_IsSome(o);

        public static bool IsNone<T>(this FSharpOption<T> o) => FSharpOption<T>.get_IsNone(o);

        public static IEnumerable<T> AsEnumerable<T>(this FSharpOption<T> o) =>
            o.IsSome() ? Enumerable.Repeat(o.Value, 1) : Enumerable.Empty<T>();

        public static T GetOr<T>(this FSharpOption<T> o, T ifNone) =>
            o.IsSome() ? o.Value : ifNone;

        public static T? AsNullable<T>(this FSharpOption<T> o) where T : struct {
            if (o.IsSome()) {
                return new T?(o.Value);
            }
            else {
                return null;
            }
        }
    }

}
