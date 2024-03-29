﻿using Microsoft.EntityFrameworkCore;
using QueryingData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace QueryingData
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeDb();

            EagerLoading_PostTag_Post();
            //EagerLoading_PostsBlogs();
            //EagerLoading_BlogsOwner_BlogsPosts();
            //EagerLoading_BlogsPostsAuthor();
            //EagerLoading_BlogsPostsAuthorPhoto();
            //EagerLoding_BlogsPostsAuthorPhoto_BlogsOwnerPhoto();
            //EagerLoding_BlogsPostsAuthor_BlogsPostsTags();

            //ExplicitLoading_BlogPostOwner();
            //ExplicitLoading_BlogPost_Count();
            //ExplicitLoading_BlogPostTag_Query();

            //SelectLoading();
        }

        #region EAGER LOADING

        static void EagerLoading_PostTag_Post()
        {
            using (var context = new BloggingContext())
            {
                var posts = context.Posts
                    .Include(post => post.Tags
                    .Where(tag => tag.Tag.TagId == "News"))
                    .ToList();
                foreach (var p in posts)
                {
                    foreach (var t in p.Tags)
                    {
                        Console.WriteLine(t.TagId);
                    }
                }
            }
        }
        static void EagerLoading_PostsBlogs()
        {
            using (var context = new BloggingContext())
            {
                var blogs = context.Blogs
                    .Include(blog => blog.Posts)
                    .ToList();
                DisplayGraph(blogs);
            }
        }

        static void EagerLoading_BlogsOwner_BlogsPosts()
        {
            using (var context = new BloggingContext())
            {
                var blogs = context.Blogs
                    .Include(blog => blog.Posts)
                    .Include(blog => blog.Owner)
                    .ToList();
                DisplayGraph(blogs);
            }
        }

        private static void EagerLoading_BlogsPostsAuthor()
        {
            using (var context = new BloggingContext())
            {
                var blogs = context.Blogs
                    .Include(blog => blog.Posts)
                     .ThenInclude(post => post.Author)
                    .ToList();
                DisplayGraph(blogs);
            }
        }

        private static void EagerLoading_BlogsPostsAuthorPhoto()
        {
            using (var context = new BloggingContext())
            {
                var blogs = context.Blogs
                    .Include(blog => blog.Posts)
                        .ThenInclude(post => post.Author)
                            .ThenInclude(author => author.Photo)
                    .ToList();
                DisplayGraph(blogs);
            }
        }

        private static void EagerLoding_BlogsPostsAuthorPhoto_BlogsOwnerPhoto()
        {
            using (var context = new BloggingContext())
            {
                var blogs = context.Blogs
                    .Include(blog => blog.Posts)
                        .ThenInclude(post => post.Author)
                            .ThenInclude(author => author.Photo)
                    .Include(blog => blog.Owner)
                        .ThenInclude(owner => owner.Photo)
                    .ToList();
                DisplayGraph(blogs);
            }
        }

        private static void EagerLoding_BlogsPostsAuthor_BlogsPostsTags()
        {
            using (var context = new BloggingContext())
            {
                var blogs = context.Blogs
                    .Include(blog => blog.Posts)
                        .ThenInclude(post => post.Author)
                    .Include(blog => blog.Posts)
                        .ThenInclude(post => post.Tags)
                    .ToList();
                DisplayGraph(blogs);
            }
        }
        #endregion

        #region EXPLICIT LOADING
        private static void ExplicitLoading_BlogPostOwner()
        {
            using (var context = new BloggingContext())
            {
                Console.WriteLine("***** User selected BlogId = 1");
                var blog = context.Blogs
                    .Single(b => b.BlogId == 1);
                Console.WriteLine($"BlogId: {blog.BlogId} - Url: {blog.Url}");

                Console.WriteLine("\n***** User will see all Posts for BlogId 1");
                context.Entry(blog)
                    .Collection(b => b.Posts)
                    .Load();
                foreach (var post in blog.Posts)
                {
                    Console.WriteLine($"\tTitle: {post.Title} - Content: {post.Content}");
                }

                Console.WriteLine("\n***** Later will the user also see name of the blog-owner");
                context.Entry(blog)
                    .Reference(b => b.Owner)
                    .Load();
                Console.WriteLine($"BlogId: {blog.BlogId} has Owner: {blog.Owner.Name}");
            }
        }

        private static void ExplicitLoading_BlogPost_Count()
        {
            using (var context = new BloggingContext())
            {
                Console.WriteLine("***** User selected BlogId = 1");
                var blog = context.Blogs
                    .Single(b => b.BlogId == 1);
                Console.WriteLine($"BlogId: {blog.BlogId} - Url: {blog.Url}");

                Console.WriteLine("\n***** User will see all Posts and the number of Posts, without actually selected them");
                var numberPosts = context.Entry(blog)
                    .Collection(b => b.Posts)
                    .Query().Count();           // No Load(), only the number is returned!


                Console.WriteLine("-----------------------------------------------------------------------");
                Console.WriteLine($"Blog {blog.BlogId} has: {numberPosts} posts");
            }
        }

        private static void ExplicitLoading_BlogPostTag_Query()
        {
            using (var context = new BloggingContext())
            {
                Console.WriteLine("***** User selected BlogId = 1");
                var blog = context.Blogs
                    .Single(b => b.BlogId == 1);
                Console.WriteLine($"BlogId: {blog.BlogId} - Url: {blog.Url}");

                Console.WriteLine("\n***** User will see all Posts and their Tags for BlogId 1");
                context.Entry(blog)
                    .Collection(b => b.Posts)
                    .Query()                // Query() opens for new queries
                    .Include(p => p.Tags)
                    .Load();

                Console.WriteLine("-----------------------------------------------------------------------");
                foreach (var post in blog.Posts)
                {
                    Console.WriteLine($"\tTitle: {post.Title} - Content: {post.Content}");
                    foreach (PostTag postTag in post.Tags)
                    {
                        Console.WriteLine($"\t\tTag: {postTag.TagId}");
                    }
                }
            }
        }
        #endregion

        #region SELECT LOADING
        private static void SelectLoading()
        {
            Console.WriteLine("---------  SelectLoading ---------");

            using (BloggingContext context = new BloggingContext())
            {
                var result = context.Blogs
                    .Where(b => b.BlogId == 2)
                    .Select(b => new
                    {
                        b.Url,
                        NumberPosts = b.Posts.Count(),
                        BlogOwner = b.Owner.Name,
                        OwnerPhotoCaption = b.Owner.Photo.Caption,
                        PostsPrBlog = b.Posts
                    }).FirstOrDefault();

                Console.WriteLine("Url: {0} - Number of Posts: {1} - BlogOwner: {2}", result.Url, result.NumberPosts, result.BlogOwner);
                foreach (var post in result.PostsPrBlog)
                {
                    Console.WriteLine($"Post title: {post.Title}");
                }
            }

        }
        #endregion

        #region INITIALIZE DB
        private static void InitializeDb()
        {
            using (var context = new BloggingContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                Console.WriteLine("Database recreated");
            }
        }
        #endregion

        #region DISPLAY GRAPH
        static void DisplayGraph(IEnumerable<Blog> blogs)
        {
            foreach (var blog in blogs)
            {
                Console.WriteLine($"BlogId: {blog.BlogId} - Url: {blog.Url} - Owner: {blog?.Owner?.Name} - PhotoCaption: {blog.Owner?.Photo?.Caption}  ");
                foreach (var post in blog.Posts)
                {
                    Console.WriteLine($"\tTitle: {post.Title} - Content: {post.Content} - PersonName: {post.Author?.Name} - PhotoCaption: {post.Author?.Photo?.Caption} ");
                    if (post.Tags != null)
                    {
                        foreach (var tag in post?.Tags)
                        {
                            Console.WriteLine($"\t\tTag: {tag?.TagId}");
                        }
                    }
                }
            }
        }
        #endregion
    }
}