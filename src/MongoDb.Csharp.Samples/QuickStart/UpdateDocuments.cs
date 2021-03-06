﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDb.Csharp.Samples.Core;
using MongoDb.Csharp.Samples.Models;
using MongoDB.Driver;
using Utils = MongoDb.Csharp.Samples.Core.Utils;

namespace MongoDb.Csharp.Samples.QuickStart
{
    public class UpdateDocuments : RunnableSample, IRunnableSample
    {
        public override Core.Samples Sample => Core.Samples.QuickStart_UpdateDocuments;
        protected override void Init()
        {
            // Create a mongodb client
            Client = new MongoClient(Utils.DefaultConnectionString);
            Utils.DropDatabase(Client, Constants.SamplesDatabase);
        }

        public async Task Run()
        {
            await UpdateSamples();
        }
        private async Task UpdateSamples()
        {
            var usersDatabase = Client.GetDatabase(Constants.SamplesDatabase);

            #region Prepare data

            // Will create the users collection on the fly if it doesn't exists
            var personsCollection = usersDatabase.GetCollection<User>(Constants.UsersCollection);

            User appPerson = RandomData.GenerateUsers(1).First();
            // Insert one document
            await personsCollection.InsertOneAsync(appPerson);


            // Insert multiple documents
            var persons = RandomData.GenerateUsers(30);

            await personsCollection.InsertManyAsync(persons);
            #endregion

            #region Typed classes commands

            // Find a person using a class filter
            var filter = Builders<User>.Filter.Eq(person => person.Id, appPerson.Id);

            // update person
            var update = Builders<User>.Update.Set(person => person.Phone, "123-456-789");
            var personUpdateResult = await personsCollection.UpdateOneAsync(filter, update);
            if (personUpdateResult.MatchedCount == 1 && personUpdateResult.ModifiedCount == 1)
            {
                Utils.Log( $"Document {appPerson.Id} Updated");
            }

            // Find multiple documents having 1200 < salary < 3500 

            var salaryFilter = Builders<User>.Filter
                .And(
                    Builders<User>.Filter.Gt(person => person.Salary, 1200),
                    Builders<User>.Filter.Lt(person => person.Salary, 3500)
                    );

            var totalPersons = await personsCollection.Find(salaryFilter).CountDocumentsAsync();

            var updateDefinition =
                Builders<User>.Update.Set(person => person.Salary, 4000);

            var updateResult = await personsCollection.UpdateManyAsync(salaryFilter, updateDefinition);

            if (updateResult.MatchedCount.Equals(totalPersons))
            {
                Utils.Log($"Salary has been updated for {totalPersons}");
            }

            #endregion

            #region BsonDocument commands
            // we need to get the BsonDocument schema based collection
            var bsonPersonCollection = usersDatabase.GetCollection<BsonDocument>(Constants.UsersCollection);
            // Find a person using a class filter
            var bsonSingleFilter = Builders<BsonDocument>.Filter.Eq("_id", appPerson.Id);
            var bsonUpdate = Builders<BsonDocument>.Update.Set("phone", "123-456-678");
            var bsonPersonUpdateResult = await bsonPersonCollection.UpdateOneAsync(bsonSingleFilter, bsonUpdate);
            if (bsonPersonUpdateResult.MatchedCount == 1 && bsonPersonUpdateResult.ModifiedCount == 1)
            {
                Utils.Log("Person updated");
            }

            var bsonSalaryFilter = Builders<BsonDocument>.Filter
                .And(
                    Builders<BsonDocument>.Filter.Gt("salary", 1200),
                    Builders<BsonDocument>.Filter.Lt("salary", 3500)
                );

            var bsonUpdateDefinition =
                Builders<BsonDocument>.Update.Set("salary", 4000);

            var bsonUpdateResult = await bsonPersonCollection.UpdateManyAsync(bsonSalaryFilter, bsonUpdateDefinition);

            #endregion

            #region Shell commands

            /*
            use Persons

            // update a single document
            db.users.updateOne({ _id: ObjectId("5e8a35e2cc20587f34f0cc48") }, { $set: {  phone: "123-456-789" } })

            // update multiple documents
            db.users.updateMany(
                { $and: [{ salary: { $gt: 1200} }, {salary: { $lt: 3500} }] },
                { $set: { salary: 4000  } }
            )

             */

            #endregion
        }
    }
}
