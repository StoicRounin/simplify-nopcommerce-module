
![Simplify Commerce](images/simplify.png) 
Simplify Commerce payment plugin for nopCommerce
================================================

The Simplify Payment payment plugin for nopCommerce is a payment method for the [nopCommerce] .Net open source shopping cart solution using [Simplify Commerce] to provide secure 
card payments.  The plugin uses card tokenization so your site never sees customer card details.  Full and partial refunds are also supported.


Installation
------------

* Download the latest version of the plugin from the releases tab.

* Extract the contents of the ZIP file and copy the `Payments.Simplify` folder to the nopCommerce `Plugins` folder on your nopCommerce website.

* On the nopCommerce Admin panel go to `Configuration > Plugins` and Click the `Reload list of plugins` button to include the new plugin.

* Locate the Simplify Commerce plugin in the list of plugins and click on the `Install` button to install the plugin:

![nopCommerce Simplify Payments plugin configuration](images/install.png)

* Once installed make sure the plugin is enabled and if not click on the `Edit` button to enable the plugin.


Configuration
-------------

To configure the plugin click the `Configure` button for the Simplify Commerce plugin on the Admin panel (go to `Configuration > Plugins`).  This displays a
page allowing you to enter the API keys necessary to make payments using the plugin.

We allow sandbox keys for testing and live keys for real payments to be entered and you can select which key pair to use by using the `Enable Live Mode` toggle.

To use Simplify Commerce's secure hosted payment form for your payments, toggle `Enable Hosted Mode` on. See [Simplify Commerce Hosted Payments] for more information.

![nopCommerce Simplify Payments plugin configuration](images/config.png)

Your API keys can be obtained by logging into the [Simplify Commerce] dashboard and copying the keys from `Settings > Api Keys`.

When done press the `Save` button to complete the configuration.

Using the plugin
----------------

To use the plugin simply select the "Simplify Commerce" payment method during the checkout process:

![nopCommerce Selection of Simplify Commerce payment method](images/payment_method.png)

If hosted payment mode is not enabled, pressing `Continue` shows the payment information page where payment details are entered:

![nopCommerce Paying with Simplify Commerce](images/payment_info.png)

Note that no card details are ever sent to your site.  Instead the details are securely sent to [Simplify Commerce] where a token 
representing the payment details is returned.

If hosted payment mode is enabled, pressing `Continue` shows a message that you will be redirected to complete the checkout process.
 
![nopCommerce Redirection message to Simplify Commerce's secure hosted payment form](images/hosted_payment_redirect_info.png)

After confirming the order, if hosted mode is enabled, you will be redirected to [Simplify Commerce's secure hosted payment form](https://www.simplify.com/commerce/docs/tools/hosted-payments) where payment details are entered.

![nopCommerce Paying with Simplify Commerce's secure hosted payment form](images/hosted_payment_info.png)

Again, no card details are ever sent to your site. You will be redirected back to the store once payment is submitted.


Simplify Commerce Dashboard
----------------------------

You can manage your Simplify account (view payments and deposits, perform refunds etc.) using the [Simplify Commerce] dashboard.

Development
-----------

The source code is located in the folder `Nop.Plugins.Payments.Simplify`.   Copy this folder to the 
`Presentation\Nop.Web\Plugins` folder of the nopCommerce project.

Compatibility
-------------

This plugin is compatible with nopCommerce 3.80, 3.70 and 3.60.

Version
-------

This is version 1.1.1 - See [CHANGES.txt](CHANGES.txt) for a list of changes.

License
-------

This software is Open Source, released under the BSD 3-Clause license. See [LICENSE.txt](LICENSE.txt) for more info.

[nopCommerce]: http://www.nopcommerce.com
[Simplify Commerce]: https://www.simplify.com
[Simplify Commerce Hosted Payments]: https://www.simplify.com/commerce/docs/tools/hosted-payments
