using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using Xunit;

namespace quick_code_test
{

    //Please fill in the implementation of the service defined below. This service is to keep track
    //of ids to return if they have been seen before. No 3rd party packages can be used and the method
    //must be thread safe to call.

    //create the implementation as efficiently as possible in both locking, memory usage, and cpu usage

    public interface IDuplicateCheckService
    {

        //checks the given id and returns true if it is the first time we have seen it
        //IT IS CRITICAL that duplicates are not allowed through this system but false
        //positives can be tolerated at a maximum error rate of less than 1%
        bool IsThisTheFirstTimeWeHaveSeen(int id);

    }

    public class HighlyOptimizedThreadSafeDuplicateCheckService : IDuplicateCheckService
    {
        private readonly MemoryCache _nonDuplicateIds =
            new MemoryCache("idCache");

        [Fact]
        public void IsThisTheFirstTimeWeHaveSeenTest()
        {
            var numberList = Enumerable.Range(1, 10000000).ToList();
            var idsToAdd = new List<int>() { 1, 2, 1, 2, 3, 4, 5, 3 };
            idsToAdd.AddRange(numberList);
            var resultsByIndex = new ConcurrentDictionary<long, bool>();

            Parallel.ForEach(idsToAdd, (item, state, index) =>
            {
                var result = IsThisTheFirstTimeWeHaveSeen(item);
                resultsByIndex.GetOrAdd(index, result);
            });
            Assert.False(resultsByIndex[0] && resultsByIndex[2], idsToAdd[0] + " has already been added and should return false.");
            Assert.False(resultsByIndex[1] && resultsByIndex[3], idsToAdd[1] + " has already been added and should return false.");
            Assert.False(resultsByIndex[4] && resultsByIndex[7], idsToAdd[4] + " has already been added and should return false.");
            Assert.True(resultsByIndex[13], idsToAdd[13] + " has not been added and should return true.");
        }

        /// <summary>
        /// Checks if the provided id has been seen before.
        /// </summary>
        /// <param name="id">Id that can not be duplicated</param>
        /// <returns>True if the value was not seen before</returns>
        public bool IsThisTheFirstTimeWeHaveSeen(int id)
        {

            if (id < 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }
            var containsId = _nonDuplicateIds.Contains(id.ToString());
            if(!containsId)
            {
                _nonDuplicateIds.Add(new CacheItem(id.ToString(), id.ToString()), new CacheItemPolicy());
            }
            return !containsId;
        }
    }
}
