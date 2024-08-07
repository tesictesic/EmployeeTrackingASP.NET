using EmployeeTrackingC_.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.Net.Http;
using System.Text.Json;
using System.Drawing.Printing;
using X.PagedList.Extensions;

namespace EmployeeTrackingC_.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly HttpClient _httpClient;
        public EmployeeController(HttpClient client)
        {
            _httpClient = client;
        }
        public async Task<IActionResult> Index(string keyword,string orderBy, int page=1, int pageSize = 10)
        {
            // napravimo listu
            List<EmployeeTable> list_employee = new List<EmployeeTable>();
            string apiKey = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            string apiUrl = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                list_employee = JsonSerializer.Deserialize<List<EmployeeTable>>(json);
				
            }
			IQueryable<EmployeeTable> list_employee_data = list_employee.AsQueryable();

			if (!string.IsNullOrEmpty(keyword)) {
                list_employee_data = list_employee_data.Where(x => x.EmployeeName != null && x.EmployeeName.ToLower().Contains(keyword.ToLower()));
            }
			if (!string.IsNullOrEmpty(orderBy))
			{
                if (orderBy != "0")
                {
                    if (orderBy=="ASC")
                    {
                        list_employee_data = list_employee_data.Where(x => x.EmployeeName != null).OrderBy(x => x.TotalDaysWorked);
                    }
                    else
                    {
                        list_employee_data = list_employee_data.Where(x => x.EmployeeName != null).OrderByDescending(x => x.TotalDaysWorked);
                    }
                }
               
			}
			
			List<EmployeeTable>list_employee_out = list_employee_data.Where(x => x.EmployeeName != null).ToList();

           

            return View(list_employee_out.ToPagedList(page, pageSize));
        }


        public async Task<IActionResult>Chart()
        {
			var filePathName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chart01.png");

			// Kreirajte direktorijum ako ne postoji
			var directoryPath = Path.GetDirectoryName(filePathName);
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
			}

			// Proverite da li slika već postoji
			if (!System.IO.File.Exists(filePathName))
			{
				var employees = await FetchEmployeeDataAsync();
				GeneratePieChart(employees, filePathName);
			}

			// Prosledi putanju slike prikazu
			ViewData["ChartImagePath"] = "/images/chart01.png";
			return View();
		}


        private async Task<List<EmployeeTable>> FetchEmployeeDataAsync()
        {
            List<EmployeeTable> employees = new List<EmployeeTable>();
            string apiKey = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            string apiUrl = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={apiKey}";

            using (var response = await _httpClient.GetAsync(apiUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    employees = JsonSerializer.Deserialize<List<EmployeeTable>>(json);
                }
            }
            return employees;
        }
		private void GeneratePieChart(List<EmployeeTable> employees, string filePath)
		{
			int chartWidth = 1600;
			int chartHeight = 1600;
			int legendWidth = 300;
			int legendHeight = 20;

			using (var bitmap = new Bitmap(chartWidth, chartHeight))
			{
				using (var graphics = Graphics.FromImage(bitmap))
				{
					graphics.Clear(Color.White);

					using (var font = new Font("Arial", 8))
					{
						// Chart title
						graphics.DrawString("Employee Time Worked", font, Brushes.Black, new PointF(10, 10));

						// Calculate total time worked
						double totalDaysWorked = employees.Sum(e => e.TotalDaysWorked);

						// Draw pie chart
						float startAngle = 0f;
						int x = 60;
						int y = 60;
						int pieWidth = chartWidth - legendWidth - 100;
						int pieHeight = chartHeight - 100;

						List<Brush> brushes = new List<Brush>
					{
						Brushes.Red, Brushes.Blue, Brushes.Green, Brushes.Yellow, Brushes.Purple, Brushes.Orange, Brushes.Brown, Brushes.Cyan
					};

						for (int i = 0; i < employees.Count; i++)
						{
							var employee = employees[i];
							float sweepAngle = (float)(employee.TotalDaysWorked / totalDaysWorked * 360);
							Brush brush = brushes[i % brushes.Count];
							graphics.FillPie(brush, x, y, pieWidth, pieHeight, startAngle, sweepAngle);
							startAngle += sweepAngle;
						}

						// Draw legend
						int legendX = chartWidth - legendWidth;
						int legendY = 30;
						for (int i = 0; i < employees.Count; i++)
						{
							var employee = employees[i];
							Brush brush = brushes[i % brushes.Count];
							graphics.FillRectangle(brush, legendX, legendY, 20, 20);
							graphics.DrawString(
								$"{employee.EmployeeName} - {(employee.TotalDaysWorked / totalDaysWorked * 100):0.00}%",
								font,
								Brushes.Black,
								new PointF(legendX + 30, legendY));
							legendY += legendHeight;
						}
					}
				}

				bitmap.Save(filePath, ImageFormat.Png);
			}
		}
	}

		
	}


    


