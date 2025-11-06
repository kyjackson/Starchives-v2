using Microsoft.AspNetCore.Components;

namespace Starchives.Components.Pages;

public partial class About : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = default!;

    // FAQ model (kept minimal)
    private class FaqQuestion
    {
        public string Title { get; set; } = "";
        public string AnswerHtml { get; set; } = "";
    }

    private List<FaqQuestion> Questions { get; } = new()
    {
        new()
        {
            Title = "Can I get the whole transcript for a video?",
            AnswerHtml =    """
							Yes. 
							<br><br> 
							On YouTube, open the video you want the transcript for and in the description, click
							"Show transcript".
							<br><br>
							<img src="img/transcript2025.png" class="figure-img img-fluid h-50" alt="Image showing how to open video transcript on YouTube.">
							"""
        },
        new()
        {
            Title = "Can I search resources other than captions from this site?",
            AnswerHtml = """
						 Currently, no. 
						 <br><br> 
						 However, I'm looking into adding the ability to search monthly reports, 
						 commlinks, roadmap roundups, and other official resources that may be of interest. This will take some time 
						 due to other obligations, so please be patient and check back on this question occasionally, as I'll
						 update this when I have more info.
						 """
		},
        new()
        {
            Title = "Can I use this tool to search captions on other channels?",
            AnswerHtml = """
						 Unfortunately, no. 
						 <br><br> 
						 The way this site works is that all relevant video info on the 
						 Star Citizen channel is stored in my database and updated automatically at regular intervals. This is 
						 to avoid quickly using up all the daily quota permitted by the Youtube Data API, and to avoid lengthy
						 response times during the retrieval of all captions matching a search query. I currently don't have the
						 resources to solve this problem in a way that would allow for more general use of this tool, but the
						 good news is the source code for this site is publicly available on
						 <a href="https://github.com/kyjackson/Starchives-v2">Github</a> under the MIT License, 
						 so feel free to fork and adapt the code however you'd like.
						 <br><br> 
						 The other factor is that I'd eventually like to include additional Star Citizen-specific resources,
						 and with this in mind, any future version of this site designed for more general use would have to be
						 under a different domain.
						 """
		},
        new()
        {
            Title = "How often is this site updated?",
            AnswerHtml = """
						 Currently the video database is updated about once per week while the site is still
						 undergoing major changes. Because of this, you may notice the quantity of views, likes, and comments 
						 displayed next to each video result are different than what Youtube says when you open the video
						 on that site. Also, the most recent videos (within 1 week) may not be available right away.
						 <br><br>
						 The site content itself is updated as frequently as I am able to
						 complete, test, and implement improvements.
						 """
		},
        new()
        {
            Title = "I still can't find what I'm looking for after several searches.",
            AnswerHtml = """
						 It's possible that what you're looking for may have been mentioned
						 in a monthly report, commlink, or some other resource that isn't a video. 
						 <br><br>
						 If you're sure this isn't the case,
						 the captions relating to your query may not exist or have been transcribed incorrectly. Check out the awesome
						 <a href="https://docs.google.com/spreadsheets/d/1_BrcpQjSPGFvn51F46PibH8ccC30RWjyI62Fyiif2pA/">SC Dev Segment Mega Index</a>, 
						 created by a fellow member of the Star Citizen community, for more detailed video info. 
						 There you can find videos organized by subject, the particular developers featured in each video, and more.
						 """
		},
        new()
        {
            Title = "What are the dashes in the captions?",
            AnswerHtml = """
						 When matching captions are found, they also include the previous and next lines in the transcript.
						 For better clarity and readability, I've separated these lines using the dashes.
						 """
		},
        new()
        {
            Title = "Where can I submit feedback?",
            AnswerHtml = """
						 Please submit all feedback to <a href=""mailto:admin@starchives.org"">admin@starchives.org</a>,
						 with the subject 'Starchives Feedback' so I can get back to you easily if necessary.
						 <br><br>
						 Alternatively, you can use the contact area below to send me a message easily and anonymously.
						 """
		},
        new()
        {
            Title = "Why do searches take so long?",
            AnswerHtml = """
						 The captions take up a large amount of space relative to all other data retrieved. Because all of this data is
						 sent from the database to the server and then from the server to the user, response times quickly get noticeably worse 
						 as more results are returned. 
						 <br><br>
						 Additionally, you may experience worse response times while there's heavy traffic on the site. 
						 For this reason, the amount of results per page is capped at 10, and the total amount of results
						 is retrieved asynchronously from the page results, to ensure that response times are kept as low as possible.
						 """
        }
    };

    // Contact form state
    private string _contactTopic = string.Empty;
    private string ContactName { get; set; } = string.Empty;
    private string ContactTopic
    {
        get => _contactTopic;
        set
        {
            if (_contactTopic == value) return;
            _contactTopic = value;
            IsTopicInvalid = false;
        }
    }
    private string ContactMessage { get; set; } = string.Empty;

    private bool IsNameInvalid { get; set; }
    private bool IsTopicInvalid { get; set; }
    private bool IsContentInvalid { get; set; }

    private bool IsSending { get; set; }
    private string ContactResultHtml { get; set; } = string.Empty;
    private string ContactButtonLabel => IsSending ? "Sending" : (HasSent ? "Sent" : "Contact");
    private bool HasSent { get; set; }

    private void ClearNameInvalid(ChangeEventArgs _ = default) => IsNameInvalid = false;
    private void ClearContentInvalid(ChangeEventArgs _ = default) => IsContentInvalid = false;

    private async Task OnSubmit()
    {
        // Validate
        IsNameInvalid = string.IsNullOrWhiteSpace(ContactName);
        IsTopicInvalid = string.IsNullOrWhiteSpace(ContactTopic);
        IsContentInvalid = string.IsNullOrWhiteSpace(ContactMessage);

        if (IsNameInvalid || IsTopicInvalid || IsContentInvalid)
        {
            return;
        }

        IsSending = true;
        ContactResultHtml = string.Empty;
        HasSent = false;
        StateHasChanged();

        try
        {
            var form = new Dictionary<string, string>
            {
                ["sender"] = ContactName,
                ["topic"] = ContactTopic,
                ["message"] = ContactMessage
            };

            using var content = new FormUrlEncodedContent(form);
            // Post to same endpoint the original site used
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await Http.PostAsync("/contact", content, cts.Token);

            if (response.IsSuccessStatusCode)
            {
                ContactResultHtml = @"<p class=""mx-auto justify-content-center""><br>Form received successfully. Thanks for your submission!</p>";
                HasSent = true;
                // keep fields if needed — original script commented out clearing
                // ContactName = ContactMessage = ContactTopic = string.Empty;
            }
            else
            {
                ContactResultHtml = @"<p class=""mx-auto justify-content-center""><br>Something went wrong. Please try again later, or submit your feedback manually <a href=""mailto:admin@starchives.org"">here</a>.</p>";
            }
        }
        catch (Exception)
        {
            ContactResultHtml = @"<p class=""mx-auto justify-content-center""><br>Something went wrong. Please try again later, or submit your feedback manually <a href=""mailto:admin@starchives.org"">here</a>.</p>";
        }
        finally
        {
            IsSending = false;
            StateHasChanged();
        }
    }
}
