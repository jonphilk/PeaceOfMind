﻿using PeaceOfMind.Data;
using PeaceOfMind.Models.DropDownLists;
using PeaceOfMind.Models.Job;
using PeaceOfMind.Models.Pet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PeaceOfMind.Services
{
    public class JobService
    {
        private readonly Guid _userId;
        private readonly ApplicationDbContext _context = new ApplicationDbContext();

        public JobService(Guid userId)
        {
            _userId = userId;
        }

        public bool CreateJob(JobCreate model)
        {

            var service = _context.Services.Find(model.ServiceId);
            TimeSpan span;

            if (service.DurationUnit == DurationUnit.Minutes)
            {
                span = new TimeSpan(0, service.Duration, 0);
            }
            else
            {
                span = new TimeSpan(service.Duration, 0, 0);
            }
                        
            var jobEntity = new Job
            {
                ClientId = model.ClientId,
                ServiceId = model.ServiceId,
                StartDate = model.StartDate,
                StartTime = model.StartTime,
                End = model.StartTime + span,
                Note = model.Note
            };

            _context.Jobs.Add(jobEntity);
            int savedChangeToJobs = _context.SaveChanges();

            var petIds = model.PetIds;
            int savedChangesToPetAssignments = 0;

            // Creating PetToJob assigments - need this becuase one job could have multiple pets and there could be multiple jobs for the same pet
            foreach (var petId in petIds)
            {
                var assignmentEntity = new PetToJob
                {
                    PetId = petId,
                    JobId = jobEntity.JobId
                };
                _context.PetsToJobs.Add(assignmentEntity);
                savedChangesToPetAssignments += _context.SaveChanges();
            }

            // CreateJob is succesful if one (from job save) plus the amount of pets on the job is equal the amount of saved changes
            return (savedChangeToJobs + savedChangesToPetAssignments == 1 + model.PetIds.Count());
        }

        public IEnumerable<JobListItem> GetJobs()
        {
            var jobs = _context.Jobs;
            List<JobListItem> jobListItems = new List<JobListItem>();

            foreach (var job in jobs)
            {
                TimeSpan span;
                if (job.Service.DurationUnit == DurationUnit.Minutes)
                {
                    span = new TimeSpan(0, job.Service.Duration, 0);
                }
                else
                {
                    span = new TimeSpan(job.Service.Duration, 0, 0);
                }
                var jobListItem = new JobListItem
                {
                    JobId = job.JobId,
                    Service = job.Service.Name,
                    StartDate = job.StartDate.ToString("d"),
                    StartTime = job.StartTime.ToString("t"),
                    Start = job.End - span,
                    End = job.End
                };
                jobListItems.Add(jobListItem);
            }

            return jobListItems;
            
        }

        public IEnumerable<JobListItem> GetTodaysJobs()
        {
            var jobs = _context.Jobs.Where(j => j.StartDate.Day == DateTime.Now.Day);
            List<JobListItem> jobListItems = new List<JobListItem>();

            if (jobs.Any())
            {

                foreach (var job in jobs)
                {
                    TimeSpan span;
                    if (job.Service.DurationUnit == DurationUnit.Minutes)
                    {
                        span = new TimeSpan(0, job.Service.Duration, 0);
                    }
                    else
                    {
                        span = new TimeSpan(job.Service.Duration, 0, 0);
                    }
                    var jobListItem = new JobListItem
                    {
                        JobId = job.JobId,
                        Service = job.Service.Name,
                        StartDate = job.StartDate.ToString("d"),
                        StartTime = job.StartTime.ToString("t"),
                        Start = job.End - span,
                        End = job.End
                    };
                    jobListItems.Add(jobListItem);
                }

                return jobListItems;
            }
            else
            {
                var noJobs = new List<JobListItem>();
                return noJobs;
            }
        }


        public JobDetail GetJobById(int id)
        {
            var jobEntity = _context.Jobs.Find(id);
            List<string> petNames = _context.PetsToJobs.Where(e => e.JobId == id).Select(e => e.Pet.Name + " ").ToList();
            List<int> petIds = _context.PetsToJobs.Where(e => e.JobId == id).Select(e => e.PetId).ToList();

            return new JobDetail
            {
                JobId = jobEntity.JobId,
                ClientId = jobEntity.ClientId,
                Client = jobEntity.Client.FirstName + " " + jobEntity.Client.LastName,
                ServiceId = jobEntity.ServiceId,
                Service = jobEntity.Service.Name,
                PetIds = petIds,
                PetNames = petNames,
                StartDate = jobEntity.StartDate.ToString("d"),
                StartTime = jobEntity.StartTime.ToString("t"),
                End = jobEntity.End.ToString("g"),
                Note = jobEntity.Note,
            };
        }

        public bool UpdateJob(JobEdit model)
        {
            var jobEntity = _context.Jobs.Find(model.JobId);

            jobEntity.ClientId = model.ClientId;
            jobEntity.ServiceId = model.ServiceId;
            jobEntity.StartDate = model.StartDate;
            jobEntity.StartTime = model.StartTime;
            jobEntity.Note = model.Note;

            int savedChangeToJobs = _context.SaveChanges();

            var petAssignments = _context.PetsToJobs.Where(e => e.JobId == model.JobId);

            // Pets on job - Is model equal to persistence?
            List<int> petIds = petAssignments.Select(e => e.PetId).ToList();
            bool areEqual = Enumerable.SequenceEqual(petIds.OrderBy(e => e), model.PetIds.OrderBy(e => e));

            // If not, delete all and reassign
            if (!areEqual)
            {
                foreach (var petAssignment in petAssignments)
                {
                    _context.PetsToJobs.Remove(petAssignment);
                }

                int savedChangesToPetAssignments = 0;
                foreach (var petId in model.PetIds)
                {
                    var assignmentEntity = new PetToJob
                    {
                        PetId = petId,
                        JobId = jobEntity.JobId
                    };
                    _context.PetsToJobs.Add(assignmentEntity);
                    savedChangesToPetAssignments += _context.SaveChanges();
                }

                return (savedChangeToJobs + savedChangesToPetAssignments == 1 + model.PetIds.Count());
            }

            return savedChangeToJobs == 1;

        }

        public bool DeleteJob(int id)
        {
            _context.Jobs.Remove(_context.Jobs.Find(id));
            return _context.SaveChanges() == 1;
        }

        // Gets pets specific to client to display in drop down list - 
        //public List<PetDropDown> GetPets(int clientId)
        //{
        //    return _context.Clients.Find(clientId).Pets.Select(e => new PetDropDown
        //    {
        //        PetId = e.PetId,
        //        Name = e.Name
        //    }).ToList();
        //}

        public List<PetDropDown> GetPets()
        {
            return _context.Pets.Select(e => new PetDropDown
            {
                PetId = e.PetId,
                Name = e.Name
            }).ToList();
        }

        public List<ClientDropDown> GetClients()
        {
            return _context.Clients.Select(e => new ClientDropDown
            {
                ClientId = e.ClientId,
                Name = e.FirstName + " " + e.LastName
            }).ToList();
        }

        public List<ServiceDropDown> GetServices()
        {
            return _context.Services.Select(e => new ServiceDropDown
            {
                ServiceId = e.ServiceId,
                Name = e.Name
            }).ToList();
        }

    }
}
