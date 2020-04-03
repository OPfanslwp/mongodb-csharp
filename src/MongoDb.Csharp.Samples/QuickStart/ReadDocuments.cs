﻿using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDb.Csharp.Samples.Core;
using MongoDb.Csharp.Samples.Models;
using MongoDB.Driver;
using Utils = MongoDb.Csharp.Samples.Core.Utils;

namespace MongoDb.Csharp.Samples.QuickStart
{
    public class ReadDocuments : RunnableSample, IRunnableSample
    {
        public override Core.Samples Sample => Core.Samples.QuickStart_ReadDocuments;
        protected override void Init()
        {
            // Create a mongodb client
            Client = new MongoClient(Utils.DefaultConnectionString);
            Utils.DropDatabase(Client, Core.Databases.Persons);
        }

        public async Task Run()
        {
            await ReadSamples();
        }
        private async Task ReadSamples()
        {
            var usersDatabase = Client.GetDatabase(Core.Databases.Persons);

            #region Prepare data

            // Will create the users collection on the fly if it doesn't exists
            var personsCollection = usersDatabase.GetCollection<User>("users");

            User appPerson = RandomData.GenerateUsers(1).First();
            // Insert one document
            await personsCollection.InsertOneAsync(appPerson);


            // Insert multiple documents
            var persons = RandomData.GenerateUsers(30);

            await personsCollection.InsertManyAsync(persons);
            #endregion

            #region Typed classes commands

            // Find a person using a class filter
            var classSingleFilter = Builders<User>.Filter.Eq(person => person.Id, appPerson.Id);
            var personInserted = await personsCollection.Find(classSingleFilter).FirstOrDefaultAsync();
            Utils.Log(personInserted.ToBsonDocument(), "Document Find with filter");

            // Find multiple documents using a filter

            var classFemaleGenderFilter = Builders<User>.Filter.Eq(person => person.Gender, Gender.Female);
            var females = await personsCollection.Find(classFemaleGenderFilter).ToListAsync();
            Utils.Log($"Found {females.Count} female persons");

            #endregion

            #region BsonDocument commands

            // Find a person using a class filter
            var bsonSingleFilter = Builders<BsonDocument>.Filter.Eq("_id", appPerson.Id);
            // we need to get the BsonDocument schema based collection
            var bsonPersonCollection = usersDatabase.GetCollection<BsonDocument>("users");
            var bsonPersonInserted = await bsonPersonCollection.Find(bsonSingleFilter).FirstOrDefaultAsync();
            bsonPersonInserted = await bsonPersonCollection.Find(new BsonDocument("_id", personInserted.Id)).FirstOrDefaultAsync();
            Utils.Log(bsonPersonInserted);
            #endregion

            #region Shell commands

            /*
            use Persons

            // find a single document
            db.users.findOne(
            {
                "_id": ObjectId("5e5d11fe152a428290f30245")
            })

            // find multiple documents
            db.users.find(
            {
                "Gender": 0
            })
             */

            #endregion
        }
    }
}
