Dislike.Me
==========
What It Is: A completely useless yet fun way to get data metrics from facebook, and possibly ruin friendships!

How It Works: We pull all your status updates from the last year until present time. We then calculate all the likes from your friends who have liked at least 2 of your posts, and sort from least amount to most.

Code is still a little rough. Not responsible for any lost friendships as a result of using this application

How To Use:

Create a new Facebook application, obtain a client id and secret, and add your variables wherever you see a PrivateSettings class respectively.

Facebook SDK handles login. Due to paging ridiculousness & the facebook feed, just used JSON.net and raw WebRequest calls to get the 'next' pages from the Graph API. 

Uses jQuery to poll status while a background thread collects JSON data, aggregates, and creates statistics. Objects then stored in cache and user is forwarded to results page. 

Dependencies:

JSON.net 

Bootstrap 3

jQuery

Facebook C# SDK

Copyright : Creative Commons Attribution-NonCommercial
http://creativecommons.org/licenses/by-nc/3.0/us/

![alt tag](https://raw.githubusercontent.com/edroper/Dislike.Me/master/Dislike.Me/layout.jpg)
