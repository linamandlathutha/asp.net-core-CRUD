using Microsoft.Azure.Cosmos;
using System.Net;
using WebApplication10.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using System.ComponentModel;
using Microsoft.AspNetCore.Http.Features;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.WindowsAzure.Storage.Table;


public class StudentRepository
{

   

    private readonly Microsoft.Azure.Cosmos.Container _container;

    public StudentRepository(CosmosClient client, string databaseName, string containerName)
    {
        Database db = Task.Run(async () => await client.CreateDatabaseIfNotExistsAsync(containerName)).Result;
        _container = Task.Run(async () => await db.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, "/studentId"))).Result;

       


    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        var query = "SELECT * FROM c";
        var students = new List<Student>();

        var feedIterator = _container.GetItemQueryIterator<Student>(query);

        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync();
            students.AddRange(response.ToList());
        }

        return students;
    }

    public async Task<Student> GetStudentByIdAsync(string id)
    {
        List<Student> Items = new List<Student>();
        try
        {
            // Build query definition
            var parameterizedQuery = new QueryDefinition(
                query: "SELECT * FROM c WHERE c.id = '" + id + "'"
            );

            // Query multiple items from container
            using FeedIterator<Student> filteredFeed = _container.GetItemQueryIterator<Student>(
                queryDefinition: parameterizedQuery
            );

            // Iterate query result pages
            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<Student> response = await filteredFeed.ReadNextAsync();

                // Iterate query results
                foreach (Student item in response)
                {
                    Items.Add(item);
                }
            }
            return Items.FirstOrDefault();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        return Items.FirstOrDefault() as Student;
    }

    public async Task<Student> CreateStudentAsync(Student student)
    {
       
        var response = await _container.CreateItemAsync(student);
        return response.Resource;
    }

    public async Task<Student> UpdateStudentAsync(Student student)
    {
        ItemResponse<Student> response = null;

        if (null == student)
            throw new ArgumentException("UpdateStudentAsync(Student) failed because: Student is null");

        if (string.IsNullOrEmpty(student.id))
            throw new ArgumentException("UpdateStudentAsync(Student) failed because: Student.id is null or empty");

        response = await _container.ReplaceItemAsync<Student>(student, student.id, new PartitionKey(student.StudentID));

        return response.Resource;
    }

    public async Task<HttpStatusCode> DeleteStudentAsync(Student item)
    {

        ItemResponse<Student> itemToDelete = await this._container.DeleteItemAsync<Student>(item.id, new PartitionKey(item.StudentID));

        if(itemToDelete.StatusCode == HttpStatusCode.NotFound)
        {
            throw new Exception("Could not find the object we want to delete by the privided model");
        }

        return itemToDelete.StatusCode;
    }

    internal string? GetStudentById(int? id)
    {
        throw new NotImplementedException();
    }








    public async Task<IEnumerable<Student>> SearchStudentsByNameAsync(string searchTerm)
    {
        var sqlQueryText = "SELECT * FROM c WHERE CONTAINS(c.studentId, @searchTerm)";
        var queryDefinition = new QueryDefinition(sqlQueryText).WithParameter("@searchTerm", searchTerm);

        var students = new List<Student>();
        var queryResultSetIterator = _container.GetItemQueryIterator<Student>(queryDefinition);

        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync();
            students.AddRange(currentResultSet.ToList());
        }

        return students;
    }

}
