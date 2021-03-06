﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medallion.OData.Parser;
using Medallion.OData.Service;
using NUnit.Framework;

namespace Medallion.OData.Tests
{
    public class ODataServiceTest
    {
        private readonly IQueryable<A> _records = new[]
        {
            new A { Id = 1, Text = "ABA", },
            new A { Id = 2, Text = "ABC", },
            new A { Id = 3, Text = "Banana", },
            new A { Id = 4, Date = DateTime.Parse("2013-12-11"), Text = string.Empty },
            new B { Id = 5, Date = DateTime.Parse("2010-9-8"), Text = "BBB" },
        }
        .AsQueryable();
            
        [Test]
        public void TestFilterQueryIdEq1() { this.TestFilterQuery("Id eq 1", null, null, null, new[] { 1 }); }
        [Test]    
        public void TestFilterQueryIdEq1Or2Or3() { this.TestFilterQuery("Id eq 1 or Id eq 2 or Id eq 3", "Id desc", "1", null, new[] { 2, 1 }); }
        [Test]    
        public void TestFilterQueryTop2() { this.TestFilterQuery("Id eq 1 or Id eq 2 or Id eq 3", "Id desc", "0", "2", new[] { 3, 2 }); }
        [Test]    
        public void TestFilterQueryStringOps() { this.TestFilterQuery("startswith(Text, 'AB') and substringof('C', Text)", null, null, null, new[] { 2 }); }
        [Test]    
        public void TestFilterQueryYear() { this.TestFilterQuery("Date ne null and year(Date) eq 2013", null, null, null, new[] { 4 }); }
        [Test]    
        public void TestFilterQueryLength() { this.TestFilterQuery("length(Text) eq 0", null, null, null, new[] { 4 }); }
        
        private void TestFilterQuery(string filter, string orderBy, string skip, string top, int[] expectedIds)
        {
            var query = ODataQueryParser.Parse(typeof(A), new NameValueCollection { { "$filter", filter }, { "$orderby", orderBy }, { "$skip", skip }, { "$top", top } });
            var results = ODataQueryFilter.Apply(this._records, query, out var inlineCountQuery);
            results.Select(a => a.Id).CollectionShouldEqual(expectedIds, orderMatters: true);
            
            var equivalentToInlineCountResults = ODataQueryFilter.Apply(this._records, query.Update(top: null, skip: 0), out _);
            inlineCountQuery.Select(a => a.Id).CollectionShouldEqual(equivalentToInlineCountResults.Select(a => a.Id));
        }

        private class A
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public double? Value { get; set; }
            public DateTime? Date { get; set; }
        }

        private class B : A
        {
            public int BId { get; set; }
        }
    }
}
