﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Pdf.Storage.Pdf;
using Pdf.Storage.Pdf.Dto;
using Protacon.NetCore.WebApi.TestUtil;
using Xunit;

namespace Pdf.Storage.Hangfire
{
    public class PdfOpenCountTests
    {
        [Fact]
        public void WhenPdfIsOpened_ThenInformationAboutOpeningIsQueruable()
        {
            var host = TestHost.Run<TestStartup>();
            var group = "default";

            var pdf = AddPdf(host, group);

            host.Get($"{pdf.PdfUri}")
                .ExpectStatusCode(HttpStatusCode.OK);

            host.Get($"/v1/usage/{group}/")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfGroupUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().Be(1);
                    x.Total.Should().Be(1);
                });

            host.Get($"/v1/usage/{group}/{pdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x =>
                {
                    x.Opened.Should().HaveCount(1);
                    x.Opened.Single().Should().BeCloseTo(DateTime.UtcNow, precision: 5000);
                });
        }

        [Fact]
        public void WhenPdfIsOpenedWithNoCountQuery_ThenDontCountIsAsOpened()
        {
            var host = TestHost.Run<TestStartup>();
            var group = Guid.NewGuid().ToString();

            var pdf = AddPdf(host, group);

            host.Get($"{pdf.PdfUri}?noCount=true");

            host.Get($"/v1/usage/{group}/")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfGroupUsageCountResponse>()
                .Passing(x =>
                {
                    x.Total.Should().Be(1);
                    x.Opened.Should().Be(0);
                });

            host.Get($"/v1/usage/{group}/{pdf.Id}.pdf")
                .ExpectStatusCode(HttpStatusCode.OK)
                .WithContentOf<PdfUsageCountResponse>()
                .Passing(x => x.Opened.Should().HaveCount(0));
        }

        private NewPdfResponse AddPdf(TestServer host, string groupId)
        {
            return host.Post($"/v1/pdf/{groupId}/",
                    new NewPdfRequest
                    {
                        Html = "<body> {{ TEXT }} </body>",
                        BaseData = new {},
                        RowData = new object[] {
                            new {}}
                    }
                ).ExpectStatusCode(HttpStatusCode.Accepted)
                .WithContentOf<NewPdfResponse[]>()
                .Select()
                .Single();
        }
    }
}
