using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using XInputDotNetPure;

public class FeedbackMail : MonoBehaviour
{
	public InputField yourEmail;
	public InputField subject;
	public InputField message;
	public Text error;

	public GameObject mainMenu;
	public GameObject tempMail;
	public GameObject mainSelected;

	bool hasController;

	public void Feedback()
	{
		StartCoroutine(SendEmail());
	}

	public void Cancel()
	{
		mainMenu.SetActive(true);
		tempMail.SetActive(false);

		if (hasController)
		{
			Cursor.lockState = CursorLockMode.Locked;
			GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
		}
	}
	
	public IEnumerator SendEmail()
	{
		yield return new WaitForSeconds(0.0f);

		if (message.text == "" || subject.text == "" || yourEmail.text == "")
		{
			error.text = "Please fill in all the fields";
		}

		else
		{
			MailMessage mail = new MailMessage();

			mail.From = new MailAddress(yourEmail.text);
			mail.To.Add("jamrootgames@gmail.com");
			mail.Subject = subject.text;
			mail.Body = "Mail from : " + yourEmail.text + " \n --------------------------------------------------- \n \n" + message.text;

			SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
			smtpServer.Port = 587;
			smtpServer.Credentials = new System.Net.NetworkCredential("jamrootgames@gmail.com", "OrangeLlama69") as ICredentialsByHost;
			smtpServer.EnableSsl = true;
			ServicePointManager.ServerCertificateValidationCallback =
				delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)

				{ return true; };

			smtpServer.Send(mail);
			error.text = "The mail is sent successfully";

			mainMenu.SetActive(true);
			tempMail.SetActive(false);

			if (hasController)
			{
				Cursor.lockState = CursorLockMode.Locked;
				GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>().SetSelectedGameObject(mainSelected);
			}
		}
	}

	void CheckController()
	{
		GamePadState gamePadState = GamePad.GetState(0);
		hasController = gamePadState.IsConnected;
	}
}









