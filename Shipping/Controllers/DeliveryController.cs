﻿// DeliveryController.cs

using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shipping.DTO.DeliveryDTOs;
using Shipping.Models;
using Shipping.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shipping.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IUnitOfWork<Delivery> unitOfWork;
        private readonly IMapper mapper;

        public DeliveryController(IUnitOfWork<Delivery> unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeliveries()
        {
            try
            {
                var deliveries = await unitOfWork.DeliveryRepository.GetAllDeliveries();
                var deliveryDTOs = mapper.Map<List<DeliveryDTO>>(deliveries);
                return Ok(deliveryDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to retrieve deliveries: {ex.Message}");
            }
        }


        // POST: api/Delivery/AddDelivery
        [HttpPost("AddDelivery")]
        public async Task<IActionResult> Add(DeliveryDTO newDeliveryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data");
            }

            try
            {
                //var newDelivery = mapper.Map<Delivery>(newDeliveryDto);
                var addedDelivery = await unitOfWork.DeliveryRepository.Insert(newDeliveryDto);
                var addedDeliveryDto = mapper.Map<DeliveryDTO>(addedDelivery);

                return Ok(addedDeliveryDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Failed to add delivery: {ex.Message}");
            }
        }

        [HttpPut("EditDelivery/{id}")]
        public async Task<IActionResult> EditDelivery(string id, DeliveryDTO updatedDeliveryDto)
        {
            var existingDelivery = await unitOfWork.DeliveryRepository.GetById(id.ToString());
            if (existingDelivery == null)
            {
                return NotFound("Delivery not found.");
            }

            mapper.Map(updatedDeliveryDto, existingDelivery);
            var editedDelivery = await unitOfWork.DeliveryRepository.EditDelivery(id, existingDelivery);
            var editedDeliveryDto = mapper.Map<DeliveryDTO>(editedDelivery);


            return Ok(editedDeliveryDto);
        }


        [HttpPut("ChangeStatus/{id}")]
        public async Task<IActionResult> ChangeDeliveryStatus(string id, bool status)
        {
            var delivery = await unitOfWork.DeliveryRepository.GetById(id);
            if (delivery == null)
            {
                return NotFound("Delivery not found.");
            }

            unitOfWork.DeliveryRepository.UpdateStatus(delivery, status);
            return NoContent();
        }

        [HttpDelete("DeleteDelivery/{id}")]
        public async Task<IActionResult> SoftDeleteDelivery(string id)
        {
            try
            {
                var delivery = await unitOfWork.DeliveryRepository.GetById(id);
                if (delivery != null)
                {
                    await unitOfWork.DeliveryRepository.SoftDeleteAsync(delivery);
                    unitOfWork.SaveChanges();
                    return Ok("Delivery deleted successfully.");
                }
                return NotFound("Delivery not found.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
