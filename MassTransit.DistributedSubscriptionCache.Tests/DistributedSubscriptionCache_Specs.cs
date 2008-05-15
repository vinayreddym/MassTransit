namespace MassTransit.DistributedSubscriptionCache.Tests
{
	using System;
	using System.Collections.Generic;
	using MassTransit.ServiceBus.Subscriptions;
	using NUnit.Framework;
	using NUnit.Framework.SyntaxHelpers;

	[TestFixture]
	public class When_using_a_distributed_subscription_cache
	{
		[Test]
		public void The_subscriptions_should_be_synchronized_between_subscription_caches()
		{
			DistributedSubscriptionCache cacheA = new DistributedSubscriptionCache();
			DistributedSubscriptionCache cacheB = new DistributedSubscriptionCache();

			const string name = "CustomMessageName";
			const string url = "http://localhost/default.html";

			Subscription sample = new Subscription(name, new Uri(url));

			cacheA.Add(sample);

			IList<Subscription> subscriptions = cacheB.List(name);

			Assert.That(subscriptions.Count, Is.EqualTo(1));
			Assert.That(subscriptions[0].MessageName, Is.EqualTo(name));
			Assert.That(subscriptions[0].EndpointUri.ToString(), Is.EqualTo(url));
		}

		[Test]
		public void Multiple_subscriptions_to_the_same_endpoint_should_be_synchronized()
		{
			DistributedSubscriptionCache cacheA = new DistributedSubscriptionCache();
			DistributedSubscriptionCache cacheB = new DistributedSubscriptionCache();

			const string name = "CustomMessageName";

			string urlA = "http://localhost/default.html";
			Subscription sample = new Subscription(name, new Uri(urlA));
			cacheA.Add(sample);

			string urlB = "http://localhost/index.html";
			sample = new Subscription(name, new Uri(urlB));
			cacheB.Add(sample);

			IList<Subscription> subscriptions = cacheA.List(name);

			Assert.That(subscriptions.Count, Is.EqualTo(2));
			Assert.That(subscriptions[0].MessageName, Is.EqualTo(name));
			Assert.That(subscriptions[0].EndpointUri.ToString(), Is.EqualTo(urlA));
			Assert.That(subscriptions[1].MessageName, Is.EqualTo(name));
			Assert.That(subscriptions[1].EndpointUri.ToString(), Is.EqualTo(urlB));
		}
		
	}
}