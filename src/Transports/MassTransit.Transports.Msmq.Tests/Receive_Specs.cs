// Copyright 2007-2008 The Apache Software Foundation.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Transports.Msmq.Tests
{
	using System;
	using System.Messaging;
	using MassTransit.Tests.Messages;
	using NUnit.Framework;
	using TestFixtures;

	[TestFixture, Category("Integration")]
	public class Receiving_an_object_from_an_endpoint :
		MsmqEndpointOnlyTestFixture
	{
		[Test]
		public void An_undecipherable_blob_should_be_discarded()
		{
			using (var queue = new MessageQueue(EndpointAddress.FormatName, QueueAccessMode.Send))
			{
				queue.Send("This is just crap, it will cause pain");
			}

			try
			{
				Endpoint.Receive(message =>
					{
						Assert.Fail("Receive should have thrown a serialization exception");

						return null;
					});
			}
			catch (Exception ex)
			{
				Assert.Fail("Did not expect " + ex.GetType() + " = " + ex.Message);
			}

			Assert.AreEqual(0, EndpointAddress.GetMessageCount(), "Endpoint was not empty");
			Assert.AreEqual(1, ErrorEndpointAddress.GetMessageCount(), "Error endpoint did not contain bogus message");
		}

		[Test]
		public void Reading_a_single_message_should_return_one_message_selector()
		{
			Endpoint.Send(new PingMessage());

			int count = 0;
			Endpoint.Receive(message =>
				{
					Assert.IsInstanceOf<PingMessage>(message);
					count++;

					return null;
				});

			Assert.AreEqual(1, count);
		}

		[Test]
		public void Reading_from_an_empty_queue_should_just_return_an_empty_enumerator()
		{
			int count = 0;
			Endpoint.Receive(message =>
				{
					count++;

					return null;
				});

			Assert.AreEqual(0, count);
		}

		[Test]
		public void Reading_without_receiving_should_return_the_same_set_of_messages()
		{
			Endpoint.Send(new PingMessage());

			int count = 0;
			Endpoint.Receive(message =>
				{
					Assert.IsInstanceOf<PingMessage>(message);
					count++;

					return null;
				});

			int secondCount = 0;
			Endpoint.Receive(message =>
				{
					Assert.IsInstanceOf<PingMessage>(message);
					secondCount++;

					return null;
				});

			Assert.AreEqual(1, count);
			Assert.AreEqual(1, secondCount);
		}

		[Test]
		public void Receiving_the_message_and_accepting_it_should_make_it_go_away()
		{
			Endpoint.Send(new PingMessage());

			Endpoint.Receive(message =>
				{
					Assert.IsInstanceOf<PingMessage>(message);

					return m => { };
				});

			int count = 0;
			Endpoint.Receive(message =>
				{
					Assert.IsInstanceOf<PingMessage>(message);
					count++;

					return null;
				});

			Assert.AreEqual(0, count);
		}
	}
}