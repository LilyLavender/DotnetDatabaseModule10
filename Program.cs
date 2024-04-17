﻿using Microsoft.EntityFrameworkCore.Query.Internal;
using NLog;
using System.IO.Compression;
using System.Linq;


namespace BlogsAndPosts
{
    class Program 
    {

        static void Main(string[] args) 
        {
            string path = Directory.GetCurrentDirectory() + "\\nlog.config";

            // create instance of Logger
            var logger = LogManager.LoadConfiguration(path).GetCurrentClassLogger();
            logger.Info("Program started");

            try {
                while (true) {
                    var db = new BloggingContext();

                    Console.WriteLine("\nType 1 to Display all blogs");
                    Console.WriteLine("Type 2 to Add Blog");
                    Console.WriteLine("Type 3 to Create Post");
                    Console.WriteLine("Type 4 to Display Posts");
                    Console.WriteLine("Type 0 to exit");
                    int option = GetInt(true, 0, 4, "", "Number must be one of the aforementioned values");
                    switch(option) {
                        case 1:
                            DisplayBlogs(db, logger);
                            break;
                        case 2:
                            AddBlog(db, logger);
                            break;
                        case 3:
                            CreatePost(db, logger);
                            break;
                        case 4:
                            DisplayPosts(db, logger);
                            break;
                        default:
                            Environment.Exit(1);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        static void DisplayBlogs(BloggingContext db, Logger logger) {
            // Display all Blogs from the database
            var query = db.Blogs.OrderBy(b => b.Name);

            Console.WriteLine("\nAll blogs in the database:");
            foreach (var item in query)
            {
                Console.WriteLine(item.Name);
            }
        }

        static void AddBlog(BloggingContext db, Logger logger) {
            // Create and save a new Blog
            Console.Write("\nEnter a name for a new Blog: ");
            var name = Console.ReadLine();
            var blog = new Blog { Name = name };

            db.AddBlog(blog);
            logger.Info("Blog added - {name}", name);
        }

        static void CreatePost(BloggingContext db, Logger logger) {
            // Get blog from user
            Blog selectedBlog = GetBlog(db);

            // Get post details from user
            String postTitle =  GetString("Enter the title of the post > ", "Post title cannot be blank.");
            String postContent = GetString("Enter the post content > ", "Post content cannot be blank.");

            // Add post to blog
            db.AddPost(new Post { Title = postTitle, Content = postContent, BlogId = selectedBlog.BlogId });
            logger.Info("Post added - {Title}", postTitle);
        }

        static void DisplayPosts(BloggingContext db, Logger logger) {
            // Get blog from user
            Blog selectedBlog = GetBlog(db);

            // Display all posts
            var posts = db.Posts.Where(p => p.BlogId == selectedBlog.BlogId).ToList();
            if (posts.Count == 0) {
                Console.WriteLine($"]n{selectedBlog.Name} is empty. Be the first to post!");
            } else {
                Console.WriteLine($"\n{posts.Count} posts found:");
                foreach (var post in posts) {
                    Console.WriteLine($"{selectedBlog.Name} - {post.Title} \n\t{post.Content}");
                }
            }
        }

        private static Blog GetBlog(BloggingContext db) {
            // Save all blogs to list
            var blogs = db.Blogs.ToList();

            // Print all blogs
            Console.WriteLine("\nAll blogs:");
            foreach (var blog in blogs) {
                Console.WriteLine($"{blog.BlogId}) {blog.Name}");
            }

            // Ask user for blog
            int minBlogId = blogs.Min(b => b.BlogId);
            int maxBlogId = blogs.Max(b => b.BlogId);

            int blogId = -1;
            Blog selectedBlog;
            while (true) {
                blogId = GetInt(true, minBlogId, maxBlogId, "Enter a blog number: ", "Invalid blog ID");
                selectedBlog = db.Blogs.Find(blogId);
                
                if (selectedBlog == null) {
                    Console.WriteLine("Invalid blog ID");
                } else {
                    break;
                }
            }

            return selectedBlog;
        }

        public static int GetInt(bool restrictValues, int intMin, int intMax, string prompt, string errorMsg) {

            string? userString = "";
            int userInt = 0;
            bool repSuccess = false;
            do {
                Console.Write(prompt);
                userString = Console.ReadLine();

                if (Int32.TryParse(userString, out userInt)) {
                    if (restrictValues)
                    {
                        if (userInt >= intMin && userInt <= intMax) {
                            repSuccess = true;
                        }
                    }
                    else
                    {
                        repSuccess = true;
                    }
                }

                // Output error
                if (!repSuccess) {
                    Console.WriteLine(errorMsg);
                }
            } while(!repSuccess);

            return userInt;

        }

        public static string GetString(string prompt, string errorMsg) {
            string? userString = "";
            bool repSuccess = false;
            do
            {
                Console.Write(prompt);
                userString = Console.ReadLine();
                if (!String.IsNullOrEmpty(userString))
                {
                    repSuccess = true;
                }
                // Output error
                if (!repSuccess)
                {
                    Console.WriteLine(errorMsg);
                }
            } while (!repSuccess);
            return userString;
        }

    }

}