﻿using InventoryManagementSystem.Helpers;
using InventoryManagementSystem.Models;
using InventoryManagementSystem.Models.Tables;
using InventoryManagementSystem.Models.ViewModels;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace InventoryManagementSystem.Controllers
{
    public class ValuesController : ApiController
    {
        public string Login(LoginViewModel model)
        {
            var db = new Context();

            var user = db.User.Where(x => x.UserName == model.Username && x.Password == model.Password ).FirstOrDefault();

            if (user != null)
            {
                /*
                var code = "teamseven";
                var hashKey = PasswordHelper.GetHashKey(code);
                var decrypt = PasswordHelper.Decrypt(hashKey, user.Password);
                if (decrypt == model.Password)
                {
                    
                }
                */

                var result = new UserViewModel();

                result.UserId = user.Id;
                result.UserName = user.UserName;
                result.Name = user.Name;

                var role = db.UserRole.Where(x => x.UserId == user.Id).FirstOrDefault();

                if (role != null)
                {
                    result.RoleId = role.RoleId;
                }

                var response = JsonConvert.SerializeObject(result);

                return response;
            }
            else
            {
                return null;
            }
        }

        public string GetUsers()
        {
            var db = new Context();
            var result = new List<UserManagementModel>();

            var users = db.User.ToList();

            foreach (var u in users)
            {
                var user = new UserManagementModel();
                user.Id = u.Id;
                user.Name = u.Name;
                user.Email = u.Email;
                user.Status = u.Active;

                var roles = db.UserRole.Where(x => x.UserId == u.Id).ToList();

                foreach (var r in roles)
                {
                    if (user.Roles == string.Empty)
                    {
                        user.Roles = r.Role.Name;
                    }
                    else
                    {
                        user.Roles += ", " + r.Role.Name;
                    }
                }

                var facilities = db.UserFacility.Where(x => x.UserId == u.Id).ToList();

                foreach (var f in facilities)
                {
                    if (user.Facilities == string.Empty)
                    {
                        user.Facilities = f.Facility.Name;
                    }
                    else
                    {
                        user.Facilities += ", " + f.Facility.Name;
                    }
                }

                result.Add(user);
            }

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetFacilities()
        {
            var db = new Context();

            var model = new List<FacilityManagementModel>();

            var result = db.Facility.Where(x => x.Status == true).ToList();
            foreach (var r in result)
            {
                var f = new FacilityManagementModel();
                f.Id = r.Id;
                f.Name = r.Name;
                f.Location = r.Location;
                f.NumberOfResources = db.Resource.Where(x => x.FacilityId == f.Id && x.Status == true).Count();

                model.Add(f);
            }

            var response = JsonConvert.SerializeObject(model);
            return response;
        }

        public string GetFacilityById(string Id)
        {
            var db = new Context();

            var facilityId = int.Parse(Id);
            var result = db.Facility.Where(x => x.Id == facilityId).FirstOrDefault();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetUser(string Id)
        {
            var db = new Context();
            var userId = int.Parse(Id);
            var result = db.User.Where(x => x.Id == userId).FirstOrDefault();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetUserByEmail(string Email)
        {
            var db = new Context();
            var result = db.User.Where(x => x.Email == Email).FirstOrDefault();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetUserFacilitiesByUserId(string Id)
        {
            var db = new Context();
            var userId = int.Parse(Id);
            var result = db.UserFacility.Where(x => x.UserId == userId).ToList();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetUserRolesByUserId(string Id)
        {
            var db = new Context();
            var userId = int.Parse(Id);
            var result = db.UserRole.Where(x => x.UserId == userId).ToList();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetResourcesByFacilityId(string Id)
        {
            var db = new Context();

            var facilityId = int.Parse(Id);
            var result = db.Resource.Where(x => x.FacilityId == facilityId && x.Status == true).ToList();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetResourceById(string Id)
        {
            var ResourceId = int.Parse(Id);
            var db = new Context();
            var result = db.Resource.Where(x => x.Id == ResourceId && x.Status == true).FirstOrDefault();

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        public string GetResourceVerificationByFacilityId(string Id)
        {
            var FacilityId = int.Parse(Id);
            var db = new Context();

            var model = new List<ResourceReport>();
            var result = db.Report.Where(x => x.Resource.FacilityId == FacilityId).ToList();

            foreach (var r in result)
            {
                var resource = new ResourceReport();
                resource.Verified = r.Verify;
                resource.Missing = r.Missing;
                resource.ResourceId = r.ResourceId;
                resource.Message = r.Message;

                var res = db.Resource.Where(x => x.Id == r.ResourceId).FirstOrDefault();
                if (res != null)
                {
                    resource.Quantity = res.Quantity;
                    resource.ResourceName = res.Name;
                }

                model.Add(resource);
            }

            var response = JsonConvert.SerializeObject(model);
            return response;
        }

        public string GetReports()
        {
            var db = new Context();
            var result = new List<ReportModel>();

            var report = db.Report.ToList();
            var resources = db.Resource.ToList();
            var facilities = db.Facility.ToList();

            foreach (var f in facilities)
            {
                if (f.Status)
                {
                    var rep = new ReportModel();
                    rep.FacilityId = f.Id;
                    rep.FacilityName = f.Name;
                    foreach (var r in report)
                    {
                        foreach (var re in resources)
                        {
                            if (re.Status)
                            {
                                if (re.Id == r.ResourceId && re.FacilityId == f.Id)
                                {
                                    var res = new ResourceReportModel();
                                    res.ReportId = r.Id;
                                    res.ResourceId = r.ResourceId;
                                    res.ResourceName = r.Resource.Name;
                                    res.MissingQuantity = r.MissingQuantity;
                                    res.QuantityChange = r.QuantityChange;
                                    res.Status = string.Empty;

                                    if (r.Verify)
                                    {
                                        res.Status = "Verified";
                                        res.Verified = true;
                                    }

                                    if (r.MissingQuantity.HasValue)
                                    {
                                        if (r.Missing)
                                        {
                                            res.Status = "Missing";
                                            res.Missing = true;
                                        }
                                    }

                                    rep.ResourceReport.Add(res);
                                }
                            }
                        }
                    }
                    result.Add(rep);
                }

            }

            var response = JsonConvert.SerializeObject(result);
            return response;
        }

        [HttpPost]
        public bool NewUser()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var user = new User();
            var facilities = new ArrayList();
            var roles = new ArrayList();

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "name":
                        user.Name = r.Value;
                        break;
                    case "email":
                        user.Email = r.Value;
                        user.UserName = r.Value;
                        break;
                    case "facilityList":
                        facilities = JsonConvert.DeserializeObject<ArrayList>(r.Value);
                        break;
                    case "roleList":
                        roles = JsonConvert.DeserializeObject<ArrayList>(r.Value);
                        break;
                }
            }

            var password = GeneratePassword();

            var code = "teamseven";
            var hashKey = PasswordHelper.GetHashKey(code);
            var encrypt = PasswordHelper.Encrypt(hashKey, password);

            user.Password = encrypt;

            user.Active = true;

            var check = db.User.Where(x => x.Email == user.Email).FirstOrDefault();

            if (check != null)
            {
                return false;
            }

            db.User.Add(user);
            SendEmail(user.Name, user.Email, password, null);

            foreach (var f in facilities)
            {
                var uf = new UserFacility();
                uf.UserId = user.Id;
                uf.FacilityId = int.Parse(f.ToString());

                db.UserFacility.Add(uf);
            }

            foreach (var r in roles)
            {
                var ur = new UserRole();
                ur.UserId = user.Id;
                ur.RoleId = int.Parse(r.ToString());

                db.UserRole.Add(ur);
            }

            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool Newfacility()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var facility = new Facility();

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "name":
                        facility.Name = r.Value;
                        break;
                    case "location":
                        facility.Location = r.Value;
                        break;
                }
            }

            db.Facility.Add(facility);

            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool UpdateUser()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var name = string.Empty;
            var email = string.Empty;
            var userId = 0;

            var facilities = new ArrayList();
            var roles = new ArrayList();

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        userId = int.Parse(r.Value);
                        break;
                    case "name":
                        name = r.Value;
                        break;
                    case "email":
                        email = r.Value;
                        break;
                    case "facilityList":
                        facilities = JsonConvert.DeserializeObject<ArrayList>(r.Value);
                        break;
                    case "roleList":
                        roles = JsonConvert.DeserializeObject<ArrayList>(r.Value);
                        break;
                }
            }

            var user = db.User.Where(x => x.Id == userId).FirstOrDefault();

            var tempEmail = user.Email;

            if (tempEmail != email)
            {
                SendEmail(string.Empty, email, string.Empty, true);
            }

            user.Name = name;
            user.Email = email;

            db.SaveChanges();

            var facilities_delete = db.UserFacility.Where(x => x.UserId == userId);
            foreach (var fd in facilities_delete)
            {
                db.UserFacility.Remove(fd);
            }

            db.SaveChanges();
            foreach (var f in facilities)
            {
                var uf = new UserFacility();
                uf.UserId = user.Id;
                uf.FacilityId = int.Parse(f.ToString());

                db.UserFacility.Add(uf);
            }

            var roles_delete = db.UserRole.Where(x => x.UserId == userId);
            foreach (var rd in roles_delete)
            {
                db.UserRole.Remove(rd);
            }

            db.SaveChanges();
            foreach (var r in roles)
            {
                var ur = new UserRole();
                ur.UserId = user.Id;
                ur.RoleId = int.Parse(r.ToString());

                db.UserRole.Add(ur);
            }

            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool DisableUser()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();
            var userId = 0;

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        userId = int.Parse(r.Value);
                        break;
                }
            }

            var user = db.User.Where(x => x.Id == userId).FirstOrDefault();
            user.Active = false;
            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool EnableUser()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();
            var userId = 0;

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        userId = int.Parse(r.Value);
                        break;
                }
            }

            var user = db.User.Where(x => x.Id == userId).FirstOrDefault();
            user.Active = true;
            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool UpdateFacility()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var name = string.Empty;
            var location = string.Empty;
            var facilityId = 0;

            var facilities = new ArrayList();
            var roles = new ArrayList();

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        facilityId = int.Parse(r.Value);
                        break;
                    case "name":
                        name = r.Value;
                        break;
                    case "location":
                        location = r.Value;
                        break;
                }
            }

            var faciliy = db.Facility.Where(x => x.Id == facilityId).FirstOrDefault();
            if (faciliy != null)
            {
                faciliy.Name = name;
                faciliy.Location = location;
            }

            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool NewResource()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var resource = new Resource();
            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "name":
                        resource.Name = r.Value;
                        break;
                    case "quantity":
                        resource.Quantity = int.Parse(r.Value);
                        break;
                    case "facilityId":
                        resource.FacilityId = int.Parse(r.Value);
                        break;
                }
            }

            db.Resource.Add(resource);
            db.SaveChanges();


            var report = new Report();
            report.ResourceId = resource.Id;

            db.Report.Add(report);
            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool UpdateResource()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var name = string.Empty;
            var quantity = string.Empty;
            var resourceId = 0;
            var UserId = 0;

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        resourceId = int.Parse(r.Value);
                        break;
                    case "name":
                        name = r.Value;
                        break;
                    case "quantity":
                        quantity = r.Value;
                        break;
                    case "UserId":
                        UserId = int.Parse(r.Value);
                        break;
                }
            }

            var res = db.Resource.Where(x => x.Id == resourceId).FirstOrDefault();
            int? difference = null;

            if (res != null)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    res.Name = name;
                }

                var oldQuantity = res.Quantity;
                var newQuantity = int.Parse(quantity);

                difference = newQuantity - oldQuantity;

                res.Quantity = int.Parse(quantity);
            }

            db.SaveChanges();

            var res_report = db.Report.Where(x => x.ResourceId == resourceId).FirstOrDefault();
            if (res_report != null)
            {
                res_report.Verify = false;
                if (UserId > 0)
                {
                    res_report.QuantityChange = difference;
                    res_report.UpdatedBy = UserId;
                }
            }
            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool ConfirmResource()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var name = string.Empty;
            var resourceId = 0;
            var UserId = 0;

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        resourceId = int.Parse(r.Value);
                        break;

                    case "UserId":
                        UserId = int.Parse(r.Value);
                        break;
                }
            }

            var res_report = db.Report.Where(x => x.ResourceId == resourceId).FirstOrDefault();
            if (res_report != null)
            {
                res_report.Verify = true;
                res_report.Missing = false;
                res_report.Message = string.Empty;
                if (UserId > 0)
                {
                    res_report.UpdatedBy = UserId;
                }
            }
            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool MissingResource()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var name = string.Empty;
            var resourceId = 0;
            var UserId = 0;
            var Quantity = 0;

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        resourceId = int.Parse(r.Value);
                        break;

                    case "Quantity":
                        Quantity = int.Parse(r.Value);
                        break;

                    case "UserId":
                        UserId = int.Parse(r.Value);
                        break;
                }
            }

            var res_report = db.Report.Where(x => x.ResourceId == resourceId).FirstOrDefault();
            if (res_report != null)
            {
                res_report.Verify = false;
                res_report.Missing = true;
                res_report.Message = string.Empty;

                res_report.MissingQuantity = Quantity;
                if (UserId > 0)
                {
                    res_report.UpdatedBy = UserId;
                }
            }
            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool DeleteFacility(string Id)
        {
            var db = new Context();
            var FacilityId = int.Parse(Id);

            var fac = db.Facility.Where(x => x.Id == FacilityId).FirstOrDefault();

            if (fac != null)
            {
                //db.Facility.Remove(fac);
                fac.Status = false;
            }

            db.SaveChanges();

            return true;
        }

        [HttpPost]
        public bool DeleteResource(string Id)
        {
            var db = new Context();
            var ResourceId = int.Parse(Id);

            var res_report = db.Report.Where(x => x.ResourceId == ResourceId).FirstOrDefault();
            if (res_report != null)
            {
                db.Report.Remove(res_report);
            }

            db.SaveChanges();

            var res = db.Resource.Where(x => x.Id == ResourceId).FirstOrDefault();

            if (res != null)
            {
                //db.Resource.Remove(res);
                res.Status = false;
            }

            db.SaveChanges();

            return true;
        }

        public bool SendMessage()
        {
            var request = Request.Content.ReadAsStringAsync().Result;
            var request_parse = JsonConvert.DeserializeObject<Dictionary<string, string>>(request);

            var db = new Context();

            var resourceId = 0;
            var message = string.Empty;
            var UserId = 0;

            foreach (var r in request_parse)
            {
                switch (r.Key)
                {
                    case "Id":
                        resourceId = int.Parse(r.Value);
                        break;
                    case "message":
                        message = r.Value;
                        break;
                    case "UserId":
                        UserId = int.Parse(r.Value);
                        break;
                }
            }

            var res_report = db.Report.Where(x => x.ResourceId == resourceId).FirstOrDefault();
            if (res_report != null)
            {
                res_report.Verify = false;
                res_report.Missing = false;
                res_report.MissingQuantity = null;

                if (UserId > 0)
                {
                    res_report.Message = message;
                    res_report.UpdatedBy = UserId;
                }
            }
            db.SaveChanges();

            return true;
        }

        private string GeneratePassword()
        {
            var result = string.Empty;
            var length = 7;
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }

            result = res.ToString();
            return result;
        }

        private async void SendEmail(string Name, string email, string password, bool? update)
        {
            var URL = "http://inventorymanagementsystem.us-west-2.elasticbeanstalk.com/";
            if (update.HasValue && update.Value)
            {
                var apiKey = "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";
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("group7@uta", "Group 7");
                var subject = "Welcome to Inventory Managment System";
                var to = new EmailAddress(email, Name);
                var plainTextContent = "Email updated";
                var htmlContent = "<p>Email updated. Login to " + URL + " with this email.</p>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);
            }
            else
            {
                var apiKey = "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";
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("group7@uta", "Group 7");
                var subject = "Welcome to Inventory Managment System";
                var to = new EmailAddress(email, Name);
                var plainTextContent = "Login in credentials. Username: " + email + " Password: " + password;
                var htmlContent = "<p>Login into with " + URL + "</p> <p>with the following credentials </p> <p>username: " + email + "</p><p>password: " + password + "</p>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);
            }
        }
    }
}
