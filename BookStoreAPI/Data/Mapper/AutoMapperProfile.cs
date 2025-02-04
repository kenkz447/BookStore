﻿using AutoMapper;
using BookStoreAPI.Data.Models;
using BookStoreAPI.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ApplicationUser, UserWithToken>();
            CreateMap<Book, BookVM>();
        }
    }
}
