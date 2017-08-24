﻿namespace TRuDI.Backend.Controllers
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.IO;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;

    using TRuDI.Backend.HanAdapter;
    using TRuDI.Backend.Models;

    public class ConnectController : Controller
    {
        private readonly ApplicationState applicationState;

        public ConnectController(ApplicationState applicationState)
        {
            this.applicationState = applicationState;
        }

        public IActionResult Index()
        {
            this.ViewData["ErrorMessage"] = this.applicationState.LastErrorMessage;
            return this.View(this.applicationState.ConnectData);
        }

        public ActionResult ManufacturerConnectForm(string deviceId)
        {
            try
            {
                this.applicationState.LoadAdapter(deviceId);

                var manufacturerParametersView = this.applicationState.ActiveHanAdapter.ManufacturerParametersView;
                if (manufacturerParametersView != null)
                {
                    this.ViewData["ManufacturerParametersViewName"] = manufacturerParametersView;
                    return this.PartialView("_ManufacturerParametersFormPartial", deviceId);
                }
            }
            catch (Exception)
            {
            }

            return this.PartialView("_ManufacturerParametersFormEmptyPartial", deviceId);
        }


        /// <summary>
        /// Gets the additional manufacturer parameters from the request.
        /// </summary>
        /// <returns>A dictionary with the manufacturer specific parameters.</returns>
        private Dictionary<string, string> GetManufacturerParametersFromRequest()
        {
            var manufacturerParameters = new Dictionary<string, string>();
            foreach (var item in this.HttpContext.Request.Form)
            {
                if (item.Key == "__RequestVerificationToken")
                {
                    continue;
                }

                if (typeof(ConnectDataViewModel).GetProperty(item.Key) == null)
                {
                    manufacturerParameters.Add(item.Key, item.Value);
                }
            }

            return manufacturerParameters;
        }

        [HttpPost]
        public async Task<IActionResult> UploadClientCert(List<IFormFile> files)
        {
            var file = Request?.Form?.Files?.FirstOrDefault();
            if (file == null)
            {
                return this.PartialView("_CertWithoutPasswordPartial", null);
            }

            var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var certData = new CertData(file.FileName, ms.ToArray());
            this.applicationState.ClientCert = certData;

            return this.VerifyCertPassword(this.applicationState.ClientCert.Password);
        }

        [HttpPost]
        public IActionResult ValidateClientCertPassword(string password)
        {
            return this.VerifyCertPassword(password);
        }
        
        private PartialViewResult VerifyCertPassword(string password)
        {
            switch (this.applicationState.ClientCert.VerifyPassword(password))
            {
                case CertPasswordState.PasswordValid:
                case CertPasswordState.WithoutPassword:
                    return this.PartialView("_CertWithoutPasswordPartial", this.applicationState.ClientCert);

                case CertPasswordState.InvalidPassword:
                    return this.PartialView("_CertPasswordInputPartial", this.applicationState.ClientCert);

                case CertPasswordState.InvalidCertFile:
                    return this.PartialView("_CertInvalidFilePartial", this.applicationState.ClientCert);
            }

            return this.PartialView("_CertWithoutPasswordPartial", this.applicationState.ClientCert);
        }

        [HttpPost]
        public IActionResult Connect(ConnectDataViewModel connectData)
        {
            this.applicationState.ManufacturerParameters = this.GetManufacturerParametersFromRequest();

            this.applicationState.ConnectData = connectData;

            if (this.applicationState.ConnectData.AuthMode == AuthMode.ClientCertificate)
            {
                if (this.applicationState.ClientCert == null
                    || this.applicationState.ClientCert.PasswordState == CertPasswordState.InvalidCertFile
                    || this.applicationState.ClientCert.PasswordState == CertPasswordState.InvalidPassword
                    || this.applicationState.ClientCert.PasswordState == CertPasswordState.NoCertSelected)
                {
                    this.applicationState.LastErrorMessage = "Kein gültiges Zertifikat angegeben.";
                    return this.RedirectToAction("Index");
                }
            }

            this.applicationState.ConnectAndLoadContracts();

            return this.RedirectToAction(
                "Index", 
                "Progress", 
                new ProgressDataViewModel
                {
                    Title = "Verbindungsaufbau",
                    GatewayImageViewName = this.applicationState.ActiveHanAdapter.GatewayImageView,
                    DeviceId = this.applicationState.ConnectData.DeviceId,
                });
        }

        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
