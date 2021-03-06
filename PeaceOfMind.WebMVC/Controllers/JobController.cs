﻿using Microsoft.AspNet.Identity;
using PeaceOfMind.Models.Job;
using PeaceOfMind.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;

namespace PeaceOfMind.WebMVC.Controllers
{
    public class JobController : Controller
    {
        private JobService CreateJobService()
        {
            var userId = Guid.Parse(User.Identity.GetUserId());
            var service = new JobService(userId);
            return service;
        }

        // GET: Job/Index view
        public ActionResult Index()
        {
            var service = CreateJobService();
            var model = service.GetJobs();
            return View(model);
        }

        public ActionResult Today()
        {
            var service = CreateJobService();
            var model = service.GetTodaysJobs();
            return View(model);
        }

        // GET: Job/Create view
        public ActionResult Create()
        {
            var service = CreateJobService();
            var clients = service.GetClients();
            var services = service.GetServices();
            var pets = service.GetPets();
            ViewBag.ClientId = new SelectList(clients, "ClientId", "Name");
            ViewBag.ServiceId = new SelectList(services, "ServiceId", "Name");
            ViewBag.PetIds = new SelectList(pets, "PetId", "Name");

            return View();
        }

        // POST: Job/Create 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JobCreate model)
        {
            if (ModelState.IsValid)
            {
                var service = CreateJobService();
                service.CreateJob(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Job/Detail
        public ActionResult Details(int id)
        {
            var service = CreateJobService();
            var model = service.GetJobById(id);

            return View(model);
        }

        // GET: Job/Edit
        public ActionResult Edit(int id)
        {
            var service = CreateJobService();
            var detail = service.GetJobById(id);
            var model =
                new JobEdit
                {
                    ClientId = detail.ClientId,
                    ServiceId = detail.ServiceId,
                    PetIds = detail.PetIds,
                    StartDate = DateTime.Parse(detail.StartDate),
                    StartTime = DateTime.Parse(detail.StartTime),
                    Note = detail.Note,
                };

            var clients = service.GetClients();
            ViewBag.Clients = clients;
            ViewBag.SelectedClient = clients.Where(client => client.ClientId == model.ClientId);

            var services = service.GetServices();
            ViewBag.Services = services;
            ViewBag.SelectedService = services.Where(svc => svc.ServiceId == model.ServiceId);

            var pets = service.GetPets();
            ViewBag.PetIds = new SelectList(pets, "PetId", "Name");

            //var pets = service.GetPets();
            //ViewBag.Pets = pets;
            //ViewBag.SelectedPets = pets.Where(pets => pets.PetIds == model.PetIds);

            return View(model);
        }

        // POST: Job/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, JobEdit model)
        {
            if (ModelState.IsValid)
            {
                var service = CreateJobService();
                service.UpdateJob(model);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Job/Delete
        [ActionName("Delete")]
        public ActionResult Delete(int id)
        {
            var service = CreateJobService();
            var model = service.GetJobById(id);

            return View(model);
        }

        // POST: Job/Delete
        [ActionName("Delete")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int id)
        {
            var service = CreateJobService();
            service.DeleteJob(id);

            return RedirectToAction("Index");
        }

        //[HttpGet]
        //public ActionResult GetEvents()
        //{
        //    var userId = Guid.Parse(User.Identity.GetUserId());
        //    var service = new JobService(userId);

        //    var events = service.GetJobs();
        //    return Json(events, JsonRequestBehavior.AllowGet);
        //}

        [HttpGet]
        public ActionResult GetEvents()
        {
            var jobEvent = new JobEvent
            {
                id = 1,
                title = "test",
                start = "2020-10-08",
                end = "2020-10-09"
            };

            return Json(jobEvent, JsonRequestBehavior.AllowGet);
        }



    }
}



// Next Steps:
// done | Populate drop down lists
// done | Figure out how to send lists through the view - checkboxes? drop down list but it lets you select multiple values?
// Parse StartDate and StartTime such that only the date and time are displayed

// Stretch - have pet ListBox populate based on chosen Client