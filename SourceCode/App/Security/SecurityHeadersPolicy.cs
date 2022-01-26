namespace ModulesRegistry.Security;

public static class SecurityHeadersPolicy
{
    public static HeaderPolicyCollection CreateSecurityHeaderCollection(IWebHostEnvironment environment)
    {
        var policy = new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .RemoveServerHeader()
            .AddCrossOriginOpenerPolicy(builder =>
             {
                 builder.SameOrigin();
             })
            .AddCrossOriginEmbedderPolicy(builder =>
            {
                builder.RequireCorp();
            })
            .AddCrossOriginResourcePolicy(builder =>
            {
                builder.SameOrigin();
            })
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().None();
                builder.AddBlockAllMixedContent();
                builder.AddImgSrc().Self().From("data:");
                builder.AddFormAction().Self();
                builder.AddFontSrc().Self();
                builder.AddStyleSrc().Self().UnsafeInline().From("https://cdn.jsdelivr.net").From("https://kit.fontawesome.com");
                builder.AddBaseUri().Self();
                builder.AddFrameAncestors().None();
                builder.AddScriptSrc().Self().UnsafeInline(); //.WithHash256(""); //.UnsafeEval();
                builder.AddUpgradeInsecureRequests();
                })
            .AddPermissionsPolicy(builder =>
            {
                builder.AddAccelerometer().None();
                builder.AddAutoplay().None();
                builder.AddCamera().None();
                builder.AddEncryptedMedia().None();
                builder.AddFullscreen().All();
                builder.AddGeolocation().None();
                builder.AddGyroscope().None();
                builder.AddMagnetometer().None();
                builder.AddMicrophone().None();
                builder.AddMidi().None();
                builder.AddPayment().None();
                builder.AddPictureInPicture().None();
                builder.AddSyncXHR().None();
                builder.AddUsb().None();
            });
        if (!environment.IsDevelopment())
        {
            policy.AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365);
        }
        return policy;
    }
}
