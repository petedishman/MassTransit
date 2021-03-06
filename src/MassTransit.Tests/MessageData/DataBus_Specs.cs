﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Tests.MessageData
{
    namespace DataBus_Specs
    {
        using System;
        using System.IO;
        using System.Security.Cryptography;
        using System.Text;
        using System.Threading.Tasks;
        using MassTransit.MessageData;
        using MassTransit.Serialization;
        using NUnit.Framework;
        using Shouldly;
        using TestFramework;


        [TestFixture]
        public class Sending_a_large_message_through_the_file_system :
            InMemoryTestFixture
        {
            [Test]
            public async Task Should_load_the_data_from_the_repository()
            {
                string data = NewId.NextGuid().ToString();

                var message = new SendMessageWithBigData
                {
                    Body = await _repository.PutString(data)
                };

                await InputQueueSendEndpoint.Send(message);

                await _received;

                _receivedBody.ShouldBe(data);
            }

            [Test]
            public async Task Should_be_able_to_write_bytes_too()
            {
                byte[] data = NewId.NextGuid().ToByteArray();

                var message = new MessageWithByteArrayImpl
                {
                    Bytes = await _repository.PutBytes(data)
                };

                await InputQueueSendEndpoint.Send(message);

                await _receivedBytes;

                _receivedBytesArray.ShouldBe(data);
            }

            IMessageDataRepository _repository;
            Task<ConsumeContext<MessageWithBigData>> _received;
            Task<ConsumeContext<MessageWithByteArray>> _receivedBytes;
            string _receivedBody;
            byte[] _receivedBytesArray;

            protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string messageDataPath = Path.Combine(baseDirectory, "MessageData");

                var dataDirectory = new DirectoryInfo(messageDataPath);

                _repository = new FileSystemMessageDataRepository(dataDirectory);

                configurator.UseMessageData<MessageWithBigData>(_repository);

                _received = Handler<MessageWithBigData>(configurator, async context =>
                {
                    _receivedBody = await context.Message.Body.Value;
                });

                configurator.UseMessageData<MessageWithByteArray>(_repository);

                _receivedBytes = Handler<MessageWithByteArray>(configurator, async context =>
                {
                    _receivedBytesArray = await context.Message.Bytes.Value;
                });
            }
        }


        [TestFixture]
        public class Sending_a_large_message_through_the_file_system_encrypted :
            InMemoryTestFixture
        {
            [Test]
            public async Task Should_load_the_data_from_the_repository()
            {
                string data = NewId.NextGuid().ToString();

                var message = new SendMessageWithBigData
                {
                    Body = await _repository.PutString(data)
                };

                await InputQueueSendEndpoint.Send(message);

                await _received;

                _receivedBody.ShouldBe(data);
            }

            [Test]
            public async Task Should_be_able_to_write_bytes_too()
            {
                byte[] data = NewId.NextGuid().ToByteArray();

                var message = new MessageWithByteArrayImpl
                {
                    Bytes = await _repository.PutBytes(data)
                };

                await InputQueueSendEndpoint.Send(message);

                await _receivedBytes;

                _receivedBytesArray.ShouldBe(data);
            }

            IMessageDataRepository _repository;
            Task<ConsumeContext<MessageWithBigData>> _received;
            Task<ConsumeContext<MessageWithByteArray>> _receivedBytes;
            string _receivedBody;
            byte[] _receivedBytesArray;

            protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string messageDataPath = Path.Combine(baseDirectory, "MessageData");

                var dataDirectory = new DirectoryInfo(messageDataPath);

                var fileRepository = new FileSystemMessageDataRepository(dataDirectory);

                ISymmetricKeyProvider keyProvider = new TestSymmetricKeyProvider();
                var cryptoStreamProvider = new AesCryptoStreamProvider(keyProvider, "default");
                _repository = new EncryptedMessageDataRepository(fileRepository, cryptoStreamProvider);

                configurator.UseMessageData<MessageWithBigData>(_repository);

                _received = Handler<MessageWithBigData>(configurator, async context =>
                {
                    _receivedBody = await context.Message.Body.Value;
                });

                configurator.UseMessageData<MessageWithByteArray>(_repository);

                _receivedBytes = Handler<MessageWithByteArray>(configurator, async context =>
                {
                    _receivedBytesArray = await context.Message.Bytes.Value;
                });
            }
        }


        [TestFixture]
        public class Receiving_a_large_message_with_data :
            InMemoryTestFixture
        {
            [Test]
            public async Task Should_load_the_data_from_the_repository()
            {
                string data = NewId.NextGuid().ToString();
                Uri dataAddress;
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data), false))
                {
                    dataAddress = await _messageDataRepository.Put(stream);
                }

                var message = new SendMessageWithBigData {Body = new ConstantMessageData<string>(dataAddress, data)};

                await InputQueueSendEndpoint.Send(message);

                await _received;

                _receivedBody.ShouldBe(data);
            }

            IMessageDataRepository _messageDataRepository;

            Task<ConsumeContext<MessageWithBigData>> _received;
            string _receivedBody;

            protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
            {
                _messageDataRepository = new InMemoryMessageDataRepository();

                configurator.UseMessageData<MessageWithBigData>(_messageDataRepository);

                _received = Handler<MessageWithBigData>(configurator, async context =>
                {
                    _receivedBody = await context.Message.Body.Value;
                });
            }
        }


        [TestFixture]
        public class Receiving_a_large_message_with_data_bytes :
            InMemoryTestFixture
        {
            [Test]
            public async Task Should_load_the_data_from_the_repository()
            {
                Guid nextGuid = NewId.NextGuid();
                string data = nextGuid.ToString();
                Uri dataAddress;
                using (var stream = new MemoryStream(nextGuid.ToByteArray(), false))
                {
                    dataAddress = await _messageDataRepository.Put(stream);
                }

                var message = new MessageWithByteArrayImpl {Bytes = new ConstantMessageData<byte[]>(dataAddress, nextGuid.ToByteArray())};

                await InputQueueSendEndpoint.Send(message);

                await _received;

                var newId = new NewId(_receivedBytesArray);
                newId.ToString().ShouldBe(data);
            }

            IMessageDataRepository _messageDataRepository;

            Task<ConsumeContext<MessageWithByteArray>> _received;
            byte[] _receivedBytesArray;

            protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
            {
                _messageDataRepository = new InMemoryMessageDataRepository();

                configurator.UseMessageData<MessageWithByteArray>(_messageDataRepository);

                _received = Handler<MessageWithByteArray>(configurator, async context =>
                {
                    _receivedBytesArray = await context.Message.Bytes.Value;
                });
            }
        }


        public interface MessageWithByteArray
        {
            MessageData<byte[]> Bytes { get; }
        }


        class MessageWithByteArrayImpl :
            MessageWithByteArray
        {
            public MessageData<byte[]> Bytes { get; set; }
        }


        public interface MessageWithBigData
        {
            MessageData<string> Body { get; }
        }


        class SendMessageWithBigData :
            MessageWithBigData
        {
            public MessageData<string> Body { get; set; }
        }
    }
}