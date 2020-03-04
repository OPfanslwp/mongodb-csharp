﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDb.Csharp.Samples.Core;
using MongoDb.Csharp.Samples.Models;
using MongoDB.Driver;
using Utils = MongoDb.Csharp.Samples.Core.Utils;

namespace MongoDb.Csharp.Samples.QuickStart
{
    public class DeleteDocuments : RunnableSample, IRunnableSample
    {
        public bool Enabled => true;
        protected override Core.Samples Sample => Core.Samples.QuickStart_DeleteDocuments;
        protected override void Init()
        {
            // Create a mongodb client
            Client = new MongoClient(Utils.DefaultConnectionString);
            Utils.DropDatabase(Client, Core.Databases.Persons);
        }

        public async Task Run()
        {
            await DeleteOperations();
        }
        private async Task DeleteOperations
            ()
        {
            var usersDatabase = Client.GetDatabase(Core.Databases.Persons);

            #region Prepare data

            // Will create the users collection on the fly if it doesn't exists
            var personsCollection = usersDatabase.GetCollection<AppPerson>("users");

            AppPerson appPerson = RandomData.GeneratePerson();
            // Insert one document
            await personsCollection.InsertOneAsync(appPerson);


            // Insert multiple documents
            var persons = new List<AppPerson>();
            for (int i = 0; i < 30; i++)
            {
                persons.Add(RandomData.GeneratePerson());
            }

            await personsCollection.InsertManyAsync(persons);
            #endregion

            #region Typed classes commands

            // Find a person using a class filter
            var filter = Builders<AppPerson>.Filter.Eq(person => person.Id, appPerson.Id);

            // delete person
            var personDeleteResult = await personsCollection.DeleteOneAsync(filter);
            if (personDeleteResult.DeletedCount == 1)
            {
                Utils.Log( $"Document {appPerson.Id} deleted");
            }

            // Find multiple documents having 1200 < salary < 3500 

            var salaryFilter = Builders<AppPerson>.Filter
                .And(
                    Builders<AppPerson>.Filter.Gt(person => person.Salary, 1200),
                    Builders<AppPerson>.Filter.Lt(person => person.Salary, 3500)
                    );

            var totalPersons = await personsCollection.Find(salaryFilter).CountDocumentsAsync();

            var personsDeleteResult = await personsCollection.DeleteManyAsync(salaryFilter);
            if (personsDeleteResult.DeletedCount.Equals(totalPersons))
            {
                Utils.Log($"{totalPersons} users deleted");
            }

            #endregion

            #region BsonDocument commands
            // we need to get the BsonDocument schema based collection
            var bsonPersonCollection = usersDatabase.GetCollection<BsonDocument>("users");
            // Find a person using a class filter
            var bsonSingleFilter = Builders<BsonDocument>.Filter.Gt("salary", 2000);
            
            var bsonPersonDeleteResult = await bsonPersonCollection.DeleteOneAsync(bsonSingleFilter);
            if (bsonPersonDeleteResult.DeletedCount == 1)
            {
                Utils.Log("Person deleted");
            }

            // delete many documents
            var bsonPersonsDeleteResult = await bsonPersonCollection.DeleteManyAsync(bsonSingleFilter);
            if (bsonPersonsDeleteResult.DeletedCount > 1)
            {
                Utils.Log($"Persons {bsonPersonDeleteResult.DeletedCount} deleted");
            }

            #endregion

            #region Shell commands

            /*
            use Persons

            // delete a single document
            db.users.deleteOne({ _id : ObjectId("5e5ff25170dc588dd0870073")})

            // delete multiple documents
            db.users.deleteMany(
                { $and: [{ salary: { $gt: 1200} }, {salary: { $lt: 3500} }] }
            )
             */

            #endregion
        }
    }
}