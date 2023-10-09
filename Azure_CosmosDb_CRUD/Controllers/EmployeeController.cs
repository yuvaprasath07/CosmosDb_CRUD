
using Azure_CosmosDb_CRUD.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Azure_CosmosDb_CRUD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly AzureMode _azureMode;
        public EmployeeController(IOptions<AzureMode> azureMode)
        {
            _azureMode = azureMode.Value;
        }

        /*private readonly string CosmosDBAccountUri = "https://yuva-db.documents.azure.com:443/";
        private readonly string CosmosDBAccountPrimaryKey = "YOSFn5acYLMIHOgIA6uBTJl8fuu59srCV9Q94lp9PSpFQ5gbSrfXPEC25A2PocaGNNJwjZXsOD7tACDbnsS79Q==";
        private readonly string CosmosDbName = "EmployeManagementDb";
        private readonly string CosmosDbContainerName = "Employee";*/


        private Microsoft.Azure.Cosmos.Container ContainerClient()
        {
            CosmosClient cosmosDbClient = new CosmosClient(_azureMode.CosmosDBAccountUri, _azureMode.CosmosDBAccountPrimaryKey);
            Microsoft.Azure.Cosmos.Container containerClient = cosmosDbClient.GetContainer(_azureMode.CosmosDbName, _azureMode.CosmosDbContainerName);
            return containerClient;
        }


        [HttpPost]
        public async Task<IActionResult> AddEmployee(EmployeeModel employee)
        {
            try
            {
                if (string.IsNullOrEmpty(employee.department))
                {
                    return BadRequest("Department is required.");
                }

                var container = ContainerClient();
                var response = await container.CreateItemAsync(employee, new PartitionKey(employee.department));
                return Ok(response.Resource); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails()
        {
            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT * FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<EmployeeModel> queryResultSetIterator = container.GetItemQueryIterator<EmployeeModel>(queryDefinition);


                List<EmployeeModel> employees = new List<EmployeeModel>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<EmployeeModel> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (EmployeeModel employee in currentResultSet)
                    {
                        employees.Add(employee);
                    }
                }

                return Ok(employees);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateEmployee(EmployeeModel emp, string partitionKey)
        {

            try
            {

                var container = ContainerClient();
                ItemResponse<EmployeeModel> res = await container.ReadItemAsync<EmployeeModel>(emp.id, new PartitionKey(partitionKey));

                var existingItem = res.Resource;
                existingItem.Name = emp.Name;
                existingItem.Country = emp.Country;
                existingItem.City = emp.City;
                existingItem.department = emp.department;
                existingItem.Designation = emp.Designation;

                var updateRes = await container.ReplaceItemAsync(existingItem, emp.id, new PartitionKey(partitionKey));

                return Ok(updateRes.Resource);

            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee(string empId, string partitionKey)
        {

            try
            {

                var container = ContainerClient();
                var response = await container.DeleteItemAsync<EmployeeModel>(empId, new PartitionKey(partitionKey));
                return Ok(response.StatusCode);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
        }
    }
}
