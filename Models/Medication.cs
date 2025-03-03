using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public class Medication
{
	public string Code { get; set; }
	public string CodeName { get; set; }
	public string CodeSystem { get; set; }
	public string StrengthValue { get; set; }
	public string StrengthUnit { get; set; }
	public Form Form { get; set; }
}
