// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Linq.Parallel.Tests
{
    public class ContainsTests
    {
        public static IEnumerable<object[]> OnlyOneData(int[] counts)
        {
            Func<int, IEnumerable<int>> positions = x => new[] { 0, x / 2, Math.Max(0, x - 1) }.Distinct();
            foreach (object[] results in UnorderedSources.Ranges(counts.Cast<int>(), positions)) yield return results;
        }

        //
        // Contains
        //
        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(UnorderedSources))]
        [MemberData(nameof(Sources.Ranges), new[] { 0, 1, 2, 16 }, MemberType = typeof(Sources))]
        public static void Contains_NoMatching(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.False(query.Contains(count));
            Assert.False(query.Contains(count, null));
            Assert.False(query.Contains(count, new ModularCongruenceComparer(count + 1)));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024 * 1024, 1024 * 1024 * 4 }, MemberType = typeof(UnorderedSources))]
        [MemberData(nameof(Sources.Ranges), new[] { 1024 * 1024, 1024 * 1024 * 4 }, MemberType = typeof(Sources))]
        public static void Contains_NoMatching_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Contains_NoMatching(labeled, count);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 16 }, MemberType = typeof(UnorderedSources))]
        [MemberData(nameof(Sources.Ranges), new[] { 16 }, MemberType = typeof(Sources))]
        public static void Contains_MultipleMatching(Labeled<ParallelQuery<int>> labeled, int count)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.True(query.Contains(count, new ModularCongruenceComparer(2)));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1024 * 1024, 1024 * 1024 * 4 }, MemberType = typeof(UnorderedSources))]
        [MemberData(nameof(Sources.Ranges), new[] { 1024 * 1024, 1024 * 1024 * 4 }, MemberType = typeof(Sources))]
        public static void Contains_MultipleMatching_Longrunning(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Contains_MultipleMatching(labeled, count);
        }

        [Theory]
        [MemberData(nameof(OnlyOneData), new[] { 2, 16 })]
        public static void Contains_OneMatching(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            ParallelQuery<int> query = labeled.Item;
            Assert.True(query.Contains(position));
            Assert.True(query.Contains(position, null));
            Assert.True(query.Contains(position, new ModularCongruenceComparer(count)));
        }

        [Theory]
        [OuterLoop]
        [MemberData(nameof(OnlyOneData), new[] { 1024 * 1024, 1024 * 1024 * 4 })]
        public static void Contains_OneMatching_Longrunning(Labeled<ParallelQuery<int>> labeled, int count, int position)
        {
            Contains_OneMatching(labeled, count, position);
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1 }, MemberType = typeof(UnorderedSources))]
        public static void Contains_OperationCanceledException_PreCanceled(Labeled<ParallelQuery<int>> labeled, int count)
        {
            CancellationTokenSource cs = new CancellationTokenSource();
            cs.Cancel();

            Functions.AssertIsCanceled(cs, () => labeled.Item.WithCancellation(cs.Token).Contains(0));
            Functions.AssertIsCanceled(cs, () => labeled.Item.WithCancellation(cs.Token).Contains(0, EqualityComparer<int>.Default));
        }

        [Theory]
        [MemberData(nameof(UnorderedSources.Ranges), new[] { 1 }, MemberType = typeof(UnorderedSources))]
        public static void Contains_AggregateException(Labeled<ParallelQuery<int>> labeled, int count)
        {
            Functions.AssertThrowsWrapped<DeliberateTestException>(() => labeled.Item.Contains(1, new FailingEqualityComparer<int>()));
        }

        [Fact]
        public static void Contains_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<bool>)null).Contains(false));
            Assert.Throws<ArgumentNullException>(() => ((ParallelQuery<bool>)null).Contains(false, EqualityComparer<bool>.Default));
        }
    }
}
