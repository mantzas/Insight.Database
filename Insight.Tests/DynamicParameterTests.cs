﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Insight.Database;
using NUnit.Framework;

namespace Insight.Tests
{
	/// <summary>
	/// Tests that dynamic objects can be sent to the database and retrieved.
	/// </summary>
	[TestFixture]
	public class DynamicParameterTests : BaseTest
	{
		[Test]
		public void TestThatDynamicObjectCanBeUsedAsParameters()
		{
			var p = new FastExpando();
			dynamic pd = p;
			pd["Int"] = 1;
			pd["Text"] = "foo";

			var list = Connection().QuerySql("SELECT Int=CONVERT (int, @Int), Text=@Text", p);

			Assert.IsNotNull(list);
			Assert.AreEqual(1, list.Count);

			dynamic result = list[0];
			Assert.AreEqual(pd["Int"], result["Int"]);
			Assert.AreEqual(pd["Text"], result["Text"]);
		}

		[Test]
		public void FastExpandoCanBeCreatedFromObject()
		{
			FastExpando o = FastExpando.FromObject(new { Id = 1, Text = "foo" });
			dynamic d = o;

			Assert.AreEqual(4, o.Count());
			Assert.AreEqual(1, d["ID"]);
			Assert.AreEqual("foo", d["Text"]);
		}

		[Test]
		public void FastExpandoCanExpandByObject()
		{
			FastExpando o = FastExpando.FromObject(new { Id = 1, Text = "foo" });
			o.Expand(new { Id = 2, Guest = "boo" });
			dynamic d = o;

			Assert.AreEqual(6, o.Count());
			Assert.AreEqual(2, d["ID"]);
			Assert.AreEqual("foo", d["Text"]);
			Assert.AreEqual("boo", d["GUEST"]);
		}

		[Test]
		public void FastExpandoCanBeUsedAsDynamic()
		{
			dynamic d = FastExpando.FromObject(new { Id = 1, Text = "foo" });
			d["Property"] = "prop";
			d.Expand(new { Id = 2, Guest = "boo" });

			FastExpando o = (FastExpando)d;
			Assert.AreEqual(7, o.Count());
			Assert.AreEqual(2, d["ID"]);
			Assert.AreEqual("foo", d["Text"]);
			Assert.AreEqual("prop", d["Property"]);
		}

		[Test]
		public void TestConvenienceExpandoQueries()
		{
			// we have two objects with parameters in them
			var o = new { Id = 1, Text = "foo" };
			var p = new { Id = 2, Guest = "boo" };

			var list = Connection().QuerySql("SELECT ID=CONVERT (int, @ID), Text=@Text, Guest=@Guest", o.Expand(p));
			FastExpando f = list[0];
			dynamic d = f;

			Assert.AreEqual(3, f.Count());
			Assert.AreEqual(2, d["ID"]);
			Assert.AreEqual("foo", d["Text"]);
			Assert.AreEqual("boo", d["GUEST"]);
		}

		[Test]
		public void TestConvenienceExpandoQueriesWithDynamicObject()
		{
			// start with a dynamic object
			dynamic o = new FastExpando();
			o["ID"] = 1;
			o["Text"] = "foo";

			// expand the dynamic
			var p = new { Id = 2, Guest = "boo" };

			var list = Connection().QuerySql("SELECT ID=CONVERT (int, @ID), Text=@Text, Guest=@Guest", (object)o.Expand(p));
			FastExpando f = list[0];
			dynamic d = f;

			Assert.AreEqual(3, f.Count());
			Assert.AreEqual(2, d["ID"]);
			Assert.AreEqual("foo", d["Text"]);
			Assert.AreEqual("boo", d["GUEST"]);
		}

		[Test]
		public void TestExpandoNullFields()
		{
			// an expando with a null value passes in null
			dynamic f = new FastExpando();
			f["Value"] = null;
			dynamic result = Connection().Query("ReflectInt", (object)f).FirstOrDefault();
			Assert.IsNull(result["Value"]);

			// an expando with a NO value passes in DEFAULT
			f = new FastExpando();
			result = Connection().Query("ReflectInt", (object)f).FirstOrDefault();
			Assert.AreEqual(5, result["Value"]);
		}

		[Test]
		public void TestThatDictionaryCanBeUsedAsParameters()
		{
			var p = new Dictionary<string, object>() { { "Int", 1 }, { "Text", "foo" } };
			var list = Connection().QuerySql("SELECT Int=CONVERT (int, @Int), Text=@Text", p);

			Assert.IsNotNull(list);
			Assert.AreEqual(1, list.Count);

			dynamic result = list[0];
			Assert.AreEqual(p["Int"], result["Int"]);
			Assert.AreEqual(p["Text"], result["Text"]);
		}

		[Test]
		public void TestGuidConversionInDynamicParameter()
		{
			var p = new FastExpando();
			dynamic pd = p;
			pd["g"] = Guid.NewGuid();

			var list = Connection().QuerySql("SELECT G=@g", p);

			Assert.IsNotNull(list);
			Assert.AreEqual(1, list.Count);

			dynamic result = list[0];
			Assert.AreEqual(pd["g"], result["g"]);
		}
	}
}
